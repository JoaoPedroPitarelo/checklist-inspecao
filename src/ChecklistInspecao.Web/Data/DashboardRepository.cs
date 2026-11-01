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

    public async Task<DashboardResumo> ObterResumoAsync(FiltroDashboard filtro, int[] regiaoIds)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        return await conn.QuerySingleAsync<DashboardResumo>(SqlResumo, Parametros(filtro, regiaoIds));
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
