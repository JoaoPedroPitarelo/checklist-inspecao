BEGIN;

CREATE TEMP TABLE exemplo_inspecao (id integer) ON COMMIT DROP;

WITH visitas (numero, dias_atras, status) AS (VALUES
    (101, 70, 'CONCLUIDA'), (101, 40, 'CONCLUIDA'), (101, 10, 'CONCLUIDA'),
    (102, 65, 'CONCLUIDA'), (102, 35, 'CONCLUIDA'), (102,  6, 'CONCLUIDA'),
    (103, 60, 'CONCLUIDA'), (103, 28, 'CONCLUIDA'),
    (104, 75, 'CONCLUIDA'), (104, 45, 'CONCLUIDA'),
    (201, 68, 'CONCLUIDA'), (201, 38, 'CONCLUIDA'), (201,  8, 'CONCLUIDA'),
    (202, 55, 'CONCLUIDA'), (202, 40, 'CONCLUIDA'), (202,  1, 'RASCUNHO'),
    (203, 62, 'CONCLUIDA'), (203, 20, 'CONCLUIDA'),
    (204, 50, 'CONCLUIDA'), (204, 15, 'CONCLUIDA'), (204,  3, 'CONCLUIDA'),
    (301, 72, 'CONCLUIDA'), (301, 42, 'CONCLUIDA'), (301, 12, 'CONCLUIDA'),
    (302, 58, 'CONCLUIDA'), (302, 25, 'CONCLUIDA'),
    (303, 80, 'CONCLUIDA'), (303, 46, 'CONCLUIDA')
),
novas AS (
    INSERT INTO inspecao (loja_id, regiao_id, gerente_nome, autor_id,
                          data_visita, observacao, status, criado_em)
    SELECT l.id,
           l.regiao_id,
           g.nome,
           u.id,
           current_date - v.dias_atras,
           'Visita de rotina (exemplo).',
           v.status,
           (current_date - v.dias_atras) + time '09:00'
    FROM visitas v
    JOIN loja           l  ON l.numero = v.numero
    JOIN gerente        g  ON g.id = l.gerente_id
    JOIN usuario_regiao ur ON ur.regiao_id = l.regiao_id
    JOIN usuario        u  ON u.id = ur.usuario_id
                          AND u.ativo
                          AND u.perfil = 'SUPERVISOR'
    RETURNING id
)
INSERT INTO exemplo_inspecao (id)
SELECT id FROM novas;


INSERT INTO inspecao_item (inspecao_id, item_checklist_id,
                           categoria_nome, categoria_ordem,
                           item_nome, item_descricao, item_ordem,
                           resposta, observacao, criado_por)
SELECT x.inspecao_id, x.item_id,
       x.categoria_nome, x.categoria_ordem,
       x.item_nome, x.item_descricao, x.item_ordem,
       x.resposta,
       CASE WHEN x.resposta = 'PENDENTE'
            THEN 'Fora do padrão: ' || lower(x.item_nome) || '. Orientado o gerente.'
       END,
       x.autor_id
FROM (
    SELECT i.id    AS inspecao_id,
           i.autor_id,
           it.id   AS item_id,
           c.nome  AS categoria_nome,
           c.ordem AS categoria_ordem,
           it.nome AS item_nome,
           it.descricao AS item_descricao,
           it.ordem AS item_ordem,
           CASE
               WHEN i.status = 'RASCUNHO' AND it.ordem > 6 THEN NULL
               WHEN h.valor < 7 THEN 'NA'
               WHEN h.valor < 7
                    + CASE WHEN it.ordem IN (5, 9, 11) THEN 30
                           WHEN it.ordem IN (13, 15)   THEN 18
                           ELSE 5 END
                    + CASE WHEN r.nome = 'Interior' THEN 10 ELSE 0 END
                    THEN 'PENDENTE'
               ELSE 'OK'
           END AS resposta
    FROM exemplo_inspecao e
    JOIN inspecao       i  ON i.id = e.id
    JOIN regiao         r  ON r.id = i.regiao_id
    CROSS JOIN item_checklist it
    JOIN categoria      c  ON c.id = it.categoria_id
    CROSS JOIN LATERAL (
        SELECT abs(hashtext(i.loja_id || '-' || i.data_visita || '-' || it.ordem)) % 100 AS valor
    ) h
    WHERE it.ativo AND c.ativo
) x;


UPDATE inspecao i
SET percentual     = round(100.0 * s.ok / NULLIF(s.ok + s.pendentes, 0), 2),
    modificado_em  = i.criado_em + interval '50 minutes',
    modificado_por = i.autor_id
FROM (
    SELECT inspecao_id,
           count(*) FILTER (WHERE resposta = 'OK')       AS ok,
           count(*) FILTER (WHERE resposta = 'PENDENTE') AS pendentes
    FROM inspecao_item
    WHERE inspecao_id IN (SELECT id FROM exemplo_inspecao)
    GROUP BY inspecao_id
) s
WHERE s.inspecao_id = i.id
  AND i.status = 'CONCLUIDA';

COMMIT;
