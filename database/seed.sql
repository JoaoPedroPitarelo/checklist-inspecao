
-- Regiões
INSERT INTO regiao (nome) VALUES
    ('Litoral'),
    ('Metropolitana'),
    ('Interior');

-- Gerentes
INSERT INTO gerente (nome) VALUES
    ('Lucas Ferreira'),
    ('Bianca Rocha'),
    ('Tiago Moura'),
    ('Juliana Reis'),
    ('André Lopes'),
    ('Patrícia Gomes'),
    ('Felipe Nunes'),
    ('Camila Duarte'),
    ('Marcos Vieira'),
    ('Aline Martins'),
    ('Rafael Souza');

-- Lojas
INSERT INTO loja (numero, nome, regiao_id, gerente_id)
SELECT d.numero, d.nome, r.id, g.id
FROM (VALUES
    (101, 'Centro',           'Litoral',       'Lucas Ferreira'),
    (102, 'Shopping Norte',   'Litoral',       'Bianca Rocha'),
    (103, 'Av. Brasil',       'Litoral',       'Tiago Moura'),
    (104, 'Orla',             'Litoral',       'Juliana Reis'),
    (201, 'Galeria',          'Metropolitana', 'André Lopes'),
    (202, 'Terminal',         'Metropolitana', 'Patrícia Gomes'),
    (203, 'Shopping Sul',     'Metropolitana', 'Felipe Nunes'),
    (204, 'Jardins',          'Metropolitana', 'Camila Duarte'),
    (301, 'Rodoviária',       'Interior',      'Marcos Vieira'),
    (302, 'Centro Histórico', 'Interior',      'Aline Martins'),
    (303, 'Outlet',           'Interior',      'Rafael Souza')
) AS d (numero, nome, regiao, gerente)
JOIN regiao  r ON r.nome = d.regiao
JOIN gerente g ON g.nome = d.gerente;

-- Usuarios
INSERT INTO usuario (login, nome, senha_hash, perfil, ativo)
SELECT d.login, d.nome, crypt('Prova@123', gen_salt('bf')), d.perfil, d.ativo
FROM (VALUES
    ('paulo.mendes', 'Paulo Mendes', 'SUPERVISOR',    true),
    ('renata.alves', 'Renata Alves', 'SUPERVISOR',    true),
    ('diego.costa',  'Diego Costa',  'SUPERVISOR',    true),
    ('helena.prado', 'Helena Prado', 'DIRETORIA',     true),
    ('carla.nunes',  'Carla Nunes',  'ADMINISTRADOR', true),
    ('sergio.lima',  'Sérgio Lima',  'SUPERVISOR',    false)
) AS d (login, nome, perfil, ativo);


-- Abrangência dos supervisores
INSERT INTO usuario_regiao (usuario_id, regiao_id)
SELECT u.id, r.id
FROM (VALUES
    ('paulo.mendes', 'Litoral'),
    ('renata.alves', 'Metropolitana'),
    ('diego.costa',  'Interior'),
    ('sergio.lima',  'Interior')
) AS d (login, regiao)
JOIN usuario u ON u.login = d.login
JOIN regiao  r ON r.nome  = d.regiao;

-- Categorias do CheckList
INSERT INTO categoria (nome, ordem) VALUES
    ('Rotina e Gestão',      1),
    ('Apresentação e VM',    2),
    ('Estoque',              3),
    ('Precificação e Vendas', 4),
    ('Pessoas e Limpeza',    5);

-- Itens do CheckList
INSERT INTO item_checklist (categoria_id, nome, descricao, ordem)
SELECT c.id, d.nome, d.descricao, d.ordem
FROM (VALUES
    (1,  'Rotina e Gestão', 'Conferência do checklist da loja', 'Checklist diário executado e assinado pelo gerente.'),
    (2,  'Rotina e Gestão', 'Reunião diária da equipe', 'Reunião de abertura realizada e registrada.'),
    (3,  'Rotina e Gestão', 'Metas diárias e indicadores', 'Metas divulgadas e indicadores do dia anterior tratados.'),
    (4,  'Apresentação e VM', 'Fachada e vitrine', 'Vitrine limpa, alinhada e com preços visíveis.'),
    (5,  'Apresentação e VM', 'Salão de vendas', 'Pilhas abastecidas, corredores livres e espelhos limpos.'),
    (6,  'Apresentação e VM', 'Expositores e paredes de calçados', 'Sem buracos de reposição e com numeração organizada.'),
    (7,  'Apresentação e VM', 'Conferência dos pares expostos', 'Pares do salão conferidos (pé direito e esquerdo).'),
    (8,  'Estoque', 'Organização do estoque', 'Organizado por seção, marca e numeração.'),
    (9,  'Estoque', 'Remanejos entre lojas', 'Remanejos enviados e recebidos no prazo.'),
    (10, 'Estoque', 'Material de almoxarifado', 'Sacolas, etiquetas e embalagens armazenadas corretamente.'),
    (11, 'Precificação e Vendas', 'Alterações de preço aplicadas', 'Alterações da semana aplicadas nas etiquetas.'),
    (12, 'Precificação e Vendas', 'Acompanhamento do atendimento', 'Atendimento observado e feedback dado.'),
    (13, 'Precificação e Vendas', 'Conferência de vendas e notas fiscais', 'Vendas do dia anterior conferidas com as notas.'),
    (14, 'Pessoas e Limpeza', 'Aparência visual da equipe', 'Equipe uniformizada e com crachá.'),
    (15, 'Pessoas e Limpeza', 'Limpeza da loja, copa e banheiros', 'Ambientes limpos, inclusive as áreas internas.')
) AS d (ordem, categoria, nome, descricao)
JOIN categoria c ON c.nome = d.categoria;


