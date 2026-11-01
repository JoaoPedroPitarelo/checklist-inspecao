using ChecklistInspecao.Web.Models;
using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class DashboardRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public DashboardRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Base de todas as consultas do dashboard: os mesmos filtros para todos os blocos
    private const string CtesFiltro = """
        WITH insp AS (
            -- Inspeções do período (pela data da visita, RN10), na abrangência e nos filtros
            SELECT i.id, i.loja_id, i.regiao_id, i.status, i.percentual
            FROM inspecao i
            WHERE i.regiao_id = ANY(@RegiaoIds)
              AND i.data_visita BETWEEN @DataInicio::date AND @DataFim::date
              AND (@RegiaoId::int IS NULL OR i.regiao_id = @RegiaoId::int)
              AND (@LojaId::int   IS NULL OR i.loja_id = @LojaId::int)
        ),
        concl AS (
            -- Indicadores consideram só as concluídas (RN10)
            SELECT * FROM insp WHERE status IN ('CONCLUIDA', 'ENCERRADA')
        ),
        lojas AS (
            -- Lojas ativas na abrangência e nos filtros (RN12)
            SELECT l.id, l.numero, l.nome, l.regiao_id
            FROM loja l
            WHERE l.ativo
              AND l.regiao_id = ANY(@RegiaoIds)
              AND (@RegiaoId::int IS NULL OR l.regiao_id = @RegiaoId::int)
              AND (@LojaId::int   IS NULL OR l.id = @LojaId::int)
        )
    """;

    // Blocos 1 e 2
    private const string SqlResumo = CtesFiltro + """
        ,
        itens AS (
            SELECT count(*) FILTER (WHERE ii.resposta = 'OK')::int       AS itens_ok,
                   count(*) FILTER (WHERE ii.resposta = 'PENDENTE')::int AS itens_pendentes,
                   count(*) FILTER (WHERE ii.resposta = 'NA')::int       AS itens_na
            FROM inspecao_item ii
            JOIN concl c ON c.id = ii.inspecao_id
        )
        SELECT (SELECT count(*) FROM insp)::int                           AS total_inspecoes,
               (SELECT count(*) FROM concl)::int                          AS concluidas,
               (SELECT count(*) FROM insp WHERE status = 'RASCUNHO')::int AS rascunhos,
               (SELECT avg(percentual) FROM concl)                        AS percentual_medio,
               (SELECT count(*) FROM lojas)::int                          AS lojas_ativas,
               (SELECT count(DISTINCT c.loja_id)
                FROM concl c
                JOIN lojas l ON l.id = c.loja_id)::int                    AS lojas_inspecionadas,
               itens.itens_ok,
               itens.itens_pendentes,
               itens.itens_na
        FROM itens
    """;

    // Bloco 3: lojas ativas sem nenhuma concluída no período (RN12)
    private const string SqlNaoInspecionadas = CtesFiltro + """

        SELECT l.numero,
            l.nome,
            r.nome AS regiao_nome,
            (SELECT max(x.data_visita)
                FROM inspecao x
                WHERE x.loja_id = l.id
                AND x.status IN ('CONCLUIDA', 'ENCERRADA')
                AND x.data_visita <= @DataFim::date) AS ultima_visita
        FROM lojas l
        JOIN regiao r ON r.id = l.regiao_id
        WHERE NOT EXISTS (SELECT 1 FROM concl c WHERE c.loja_id = l.id)
        ORDER BY ultima_visita NULLS FIRST, l.numero
    """;

    // Bloco 4: os 5 itens com mais pendências nas concluídas do período
    private const string SqlTopPendencias = CtesFiltro + """

        SELECT ic.nome                        AS item_nome,
            count(*)::int                  AS pendencias,
            count(DISTINCT c.loja_id)::int AS lojas_afetadas
        FROM inspecao_item ii
        JOIN concl c           ON c.id = ii.inspecao_id
        JOIN item_checklist ic ON ic.id = ii.item_checklist_id
        WHERE ii.resposta = 'PENDENTE'
        GROUP BY ii.item_checklist_id, ic.nome
        ORDER BY pendencias DESC, lojas_afetadas DESC, ic.nome
        LIMIT 5
    """;

    // Bloco 5: indicadores por região
    private const string SqlPorRegiao = CtesFiltro + """

        SELECT r.nome AS regiao_nome,
            (SELECT count(*)
                FROM lojas l
                WHERE l.regiao_id = r.id)::int                 AS lojas,
            (SELECT count(DISTINCT c.loja_id)
                FROM concl c
                JOIN lojas l ON l.id = c.loja_id
                WHERE l.regiao_id = r.id)::int                 AS lojas_inspecionadas,
            (SELECT count(*)
                FROM concl c
                WHERE c.regiao_id = r.id)::int                 AS inspecoes,
            (SELECT avg(c.percentual)
                FROM concl c
                WHERE c.regiao_id = r.id)                      AS percentual_medio,
            (SELECT count(*)
                FROM concl c
                JOIN inspecao_item ii ON ii.inspecao_id = c.id
                WHERE c.regiao_id = r.id
                AND ii.resposta = 'PENDENTE')::int           AS pendencias
        FROM regiao r
        WHERE r.id = ANY(@RegiaoIds)
        AND (@RegiaoId::int IS NULL OR r.id = @RegiaoId::int)
        ORDER BY r.nome
    """;

    public async Task<DashboardResumo> ObterResumoAsync(FiltroDashboard filtro, int[] regiaoIds)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        return await conn.QuerySingleAsync<DashboardResumo>(SqlResumo, Parametros(filtro, regiaoIds));
    }

    public async Task<IReadOnlyList<LojaNaoInspecionada>> ListarNaoInspecionadasAsync(FiltroDashboard filtro, int[] regiaoIds)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var lojas = await conn.QueryAsync<LojaNaoInspecionada>(SqlNaoInspecionadas, Parametros(filtro, regiaoIds));
        return lojas.ToList();
    }

    public async Task<IReadOnlyList<ItemPendencia>> ListarTopPendenciasAsync(FiltroDashboard filtro, int[] regiaoIds)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var itens = await conn.QueryAsync<ItemPendencia>(SqlTopPendencias, Parametros(filtro, regiaoIds));
        return itens.ToList();
    }

    public async Task<IReadOnlyList<ConsolidadoRegiao>> ListarPorRegiaoAsync(FiltroDashboard filtro, int[] regiaoIds)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        var regioes = await conn.QueryAsync<ConsolidadoRegiao>(SqlPorRegiao, Parametros(filtro, regiaoIds));
        return regioes.ToList();
    }

    private static object Parametros(FiltroDashboard filtro, int[] regiaoIds) => new
    {
        RegiaoIds = regiaoIds,
        filtro.DataInicio,
        filtro.DataFim,
        filtro.RegiaoId,
        filtro.LojaId
    };
}
