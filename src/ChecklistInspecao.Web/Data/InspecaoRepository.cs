using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class InspecaoRepository
{
    private readonly NpgsqlDataSource _dataSource;

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
}