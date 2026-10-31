CREATE TABLE usuario (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    login          text        NOT NULL UNIQUE,
    nome           text        NOT NULL,
    senha_hash     text        NOT NULL,
    perfil         text        NOT NULL
                   CHECK (perfil IN ('SUPERVISOR', 'DIRETORIA', 'ADMINISTRADOR')), -- Verifica se o conteúdo de perfil está entre os valores
    ativo          boolean     NOT NULL DEFAULT true,
    criado_em      timestamptz NOT NULL DEFAULT now(), -- timestamptz é um time stamp com timezone
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE regiao (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome           text        NOT NULL UNIQUE,
    criado_em      timestamptz NOT NULL DEFAULT now(),
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE usuario_regiao (
    usuario_id integer NOT NULL REFERENCES usuario (id),
    regiao_id  integer NOT NULL REFERENCES regiao (id),
    PRIMARY KEY (usuario_id, regiao_id)
);

CREATE TABLE gerente (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome           text        NOT NULL,
    ativo          boolean     NOT NULL DEFAULT true,
    criado_em      timestamptz NOT NULL DEFAULT now(),
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE loja (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    numero         integer     NOT NULL UNIQUE,
    nome           text        NOT NULL,
    regiao_id      integer     NOT NULL REFERENCES regiao (id),
    gerente_id     integer     NOT NULL UNIQUE REFERENCES gerente (id),
    ativo          boolean     NOT NULL DEFAULT true,
    criado_em      timestamptz NOT NULL DEFAULT now(),
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE categoria_item (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome           text        NOT NULL UNIQUE,
    ordem          integer     NOT NULL,
    ativo          boolean     NOT NULL DEFAULT true,
    criado_em      timestamptz NOT NULL DEFAULT now(),
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE item_checklist (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categoria_id   integer     NOT NULL REFERENCES categoria_item (id),
    nome           text        NOT NULL,
    descricao      text        NOT NULL,
    ordem          integer     NOT NULL,
    ativo          boolean     NOT NULL DEFAULT true,
    criado_em      timestamptz NOT NULL DEFAULT now(),
    criado_por     integer     REFERENCES usuario (id),
    modificado_em  timestamptz,
    modificado_por integer     REFERENCES usuario (id)
);

CREATE TABLE inspecao (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    loja_id        integer      NOT NULL REFERENCES loja (id),
    regiao_id      integer      NOT NULL REFERENCES regiao (id),
    gerente_nome   text         NOT NULL,
    autor_id       integer      NOT NULL REFERENCES usuario (id),
    data_visita    date         NOT NULL,
    observacao     text,
    status         text         NOT NULL DEFAULT 'RASCUNHO'
                   CHECK (status IN ('RASCUNHO', 'CONCLUIDA', 'ENCERRADA')),
    percentual     numeric(5,2) CHECK (percentual BETWEEN 0 AND 100),
    criado_em      timestamptz  NOT NULL DEFAULT now(),
    modificado_em  timestamptz,
    modificado_por integer      REFERENCES usuario (id)
);

CREATE TABLE inspecao_item (
    id                integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    inspecao_id       integer     NOT NULL REFERENCES inspecao (id),
    item_checklist_id integer     NOT NULL REFERENCES item_checklist (id),
    categoria_nome    text        NOT NULL,
    categoria_ordem   integer     NOT NULL,
    item_nome         text        NOT NULL,
    item_descricao    text        NOT NULL,
    item_ordem        integer     NOT NULL,
    status            text        CHECK (status IN ('OK', 'PENDENTE', 'NA')),
    observacao        text,
    criado_em         timestamptz NOT NULL DEFAULT now(),
    criado_por        integer     REFERENCES usuario (id),
    modificado_em     timestamptz,
    modificado_por    integer     REFERENCES usuario (id),
    UNIQUE (inspecao_id, item_checklist_id)
);


CREATE INDEX ix_inspecao_data_visita ON inspecao (data_visita);
CREATE INDEX ix_inspecao_regiao      ON inspecao (regiao_id);
CREATE INDEX ix_inspecao_loja        ON inspecao (loja_id);
CREATE INDEX ix_inspecao_autor       ON inspecao (autor_id);
