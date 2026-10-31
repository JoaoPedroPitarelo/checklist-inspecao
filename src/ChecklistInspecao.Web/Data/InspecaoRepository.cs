using ChecklistInspecao.Web.Models;
using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class InspecaoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    private const string SqlAtualizarItem = """
        UPDATE inspecao_item
        SET resposta = @Resposta,
            observacao = @Observacao,
            modificado_em = now(),
            modificado_por = @UsuarioId
        WHERE id = @Id
            AND inspecao_id = @InspecaoId
            AND (resposta IS DISTINCT FROM @Resposta
                OR observacao IS DISTINCT FROM @Observacao)
    """;

    private const string SqlContarHistorico = "SELECT count(*) " + FiltrosHistorico;

    private const string SqlListarHistorico = """
        SELECT i.id,
            i.data_visita,
            l.numero AS loja_numero,
            l.nome   AS loja_nome,
            r.nome   AS regiao_nome,
            a.nome   AS autor_nome,
            i.status,
            i.percentual

        """ + FiltrosHistorico + """

        ORDER BY i.data_visita DESC, i.id DESC
        LIMIT @Limite OFFSET @Offset
    """;

    private const string FiltrosHistorico = """
        FROM inspecao i
            JOIN loja    l ON l.id = i.loja_id
            JOIN regiao  r ON r.id = i.regiao_id
            JOIN usuario a ON a.id = i.autor_id
        WHERE i.regiao_id = ANY(@RegiaoIds)
        AND (@DataInicio::date IS NULL OR i.data_visita >= @DataInicio::date)
        AND (@DataFim::date    IS NULL OR i.data_visita <= @DataFim::date)
        AND (@RegiaoId::int    IS NULL OR i.regiao_id = @RegiaoId::int)
        AND (@LojaId::int      IS NULL OR i.loja_id = @LojaId::int)
        AND (@Status::text     IS NULL OR i.status = @Status::text)
        """;

    private static Task<int> AtualizarItensAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, int inspecaoId, int usuarioId, 
        IEnumerable<ItemRespostaInput> itens)
    {
        var parametros = itens.Select(i => new
        {
            i.Id,
            i.Resposta,
            i.Observacao,
            InspecaoId = inspecaoId,
            UsuarioId = usuarioId
        });

        return conn.ExecuteAsync(SqlAtualizarItem, parametros, tx);
    }

    public InspecaoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<int?> CriarAsync(int lojaId, DateTime dataVisita, string? observacao, int autorId, int[] regiaoIds)
    {
        const string sqlInspecao = """
            INSERT INTO inspecao (loja_id, regiao_id, gerente_nome, autor_id, data_visita, observacao)
            SELECT l.id, l.regiao_id, g.nome, @AutorId, @DataVisita, @Observacao
            FROM loja l
            JOIN gerente g ON g.id = l.gerente_id
            WHERE l.id = @LojaId
              AND l.ativo
              AND l.regiao_id = ANY(@RegiaoIds)
            RETURNING id
        """;

        // Cópia dos itens e categorias ativos no momento da criação da inspecao
        const string sqlItens = """
            INSERT INTO inspecao_item (inspecao_id, item_checklist_id,
                                       categoria_nome, categoria_ordem,
                                       item_nome, item_descricao, item_ordem,
                                       criado_por)
            SELECT @InspecaoId, i.id,
                   c.nome, c.ordem,
                   i.nome, i.descricao, i.ordem,
                   @AutorId
            FROM item_checklist i
            JOIN categoria c ON c.id = i.categoria_id
            WHERE i.ativo AND c.ativo
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var inspecaoId = await conn.QuerySingleOrDefaultAsync<int?>(sqlInspecao, new
        {
            LojaId = lojaId,
            DataVisita = dataVisita.Date,
            Observacao = observacao,
            AutorId = autorId,
            RegiaoIds = regiaoIds
        }, tx);

        if (inspecaoId is null)
        {
            return null;
        }

        await conn.ExecuteAsync(sqlItens, new { InspecaoId = inspecaoId, AutorId = autorId }, tx);

        await tx.CommitAsync();
        return inspecaoId;
    }

    public async Task<InspecaoDetalhe?> ObterAsync(int id, int[] regiaoIds)
    {
        const string sqlInspecao = """
            SELECT i.id,
                l.numero AS loja_numero,
                l.nome AS loja_nome,
                r.nome AS regiao_nome,
                i.gerente_nome,
                i.autor_id,
                a.nome AS autor_nome,
                i.data_visita,
                i.observacao,
                i.status,
                i.percentual,
                i.criado_em,
                i.modificado_em,
                m.nome AS modificado_por_nome
            FROM inspecao i
                JOIN loja    l ON l.id = i.loja_id
                JOIN regiao  r ON r.id = i.regiao_id
                JOIN usuario a ON a.id = i.autor_id
                LEFT JOIN usuario m ON m.id = i.modificado_por
            WHERE i.id = @Id
                AND i.regiao_id = ANY(@RegiaoIds)
        """;

        const string sqlItens = """
            SELECT id,
                categoria_nome,
                categoria_ordem,
                item_nome,
                item_descricao,
                item_ordem,
                resposta,
                observacao
            FROM inspecao_item
                WHERE inspecao_id = @Id
            ORDER BY categoria_ordem, item_ordem
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();

        var inspecao = await conn.QuerySingleOrDefaultAsync<InspecaoDetalhe>(
            sqlInspecao, new { Id = id, RegiaoIds = regiaoIds });

        if (inspecao is null)
        {
            return null;
        }

        var itens = await conn.QueryAsync<InspecaoItem>(sqlItens, new { Id = id });
        inspecao.Itens = itens.ToList();
        return inspecao;
    }

    public async Task<bool> SalvarRespostasAsync(int inspecaoId, int usuarioId, IEnumerable<ItemRespostaInput> itens)
    {
        const string sqlInspecao = """
            UPDATE inspecao
            SET modificado_em = now(),
                modificado_por = @UsuarioId
            WHERE id = @InspecaoId
                AND autor_id = @UsuarioId
                AND status = 'RASCUNHO'
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var linhas = await conn.ExecuteAsync(sqlInspecao, new { InspecaoId = inspecaoId, UsuarioId = usuarioId }, tx);
        if (linhas == 0)
        {
            return false;
        }

        await AtualizarItensAsync(conn, tx, inspecaoId, usuarioId, itens);

        await tx.CommitAsync();
        return true;
    }
   
    public async Task<bool> ConcluirAsync(int inspecaoId, int usuarioId, 
        IEnumerable<ItemRespostaInput> itens, decimal? percentual)
    {
        const string sqlInspecao = """
            UPDATE inspecao
            SET status = 'CONCLUIDA',
                percentual = @Percentual,
                modificado_em = now(),
                modificado_por = @UsuarioId
            WHERE id = @InspecaoId
                AND autor_id = @UsuarioId
                AND status = 'RASCUNHO'
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var linhas = await conn.ExecuteAsync(sqlInspecao, new
        {
            InspecaoId = inspecaoId,
            UsuarioId = usuarioId,
            Percentual = percentual
        }, tx);

        if (linhas == 0)
        {
            return false;
        }

        await AtualizarItensAsync(conn, tx, inspecaoId, usuarioId, itens);

        await tx.CommitAsync();
        return true;
    }

    public async Task<(IReadOnlyList<InspecaoResumo> Itens, int Total)> ListarHistoricoAsync(
        FiltroHistorico filtro, int[] regiaoIds, int pagina, int tamanhoPagina)
    {
        var parametros = new
        {
            RegiaoIds = regiaoIds,
            filtro.DataInicio,
            filtro.DataFim,
            filtro.RegiaoId,
            filtro.LojaId,
            filtro.Status,
            Limite = tamanhoPagina,
            Offset = (pagina - 1) * tamanhoPagina
        };

        await using var conn = await _dataSource.OpenConnectionAsync();

        var total = await conn.ExecuteScalarAsync<int>(SqlContarHistorico, parametros);
        var itens = await conn.QueryAsync<InspecaoResumo>(SqlListarHistorico, parametros);

        return (itens.ToList(), total);
    }
}
