using Microsoft.Data.Sqlite;
using AppEmbarcado.Models;

namespace AppEmbarcado.Services
{
    public sealed class ArmazenamentoSqlite : IArmazenamentoLeituras
    {
        private readonly string conexao;

        public ArmazenamentoSqlite(string caminhoArquivo)
        {
            conexao = $"Data Source={caminhoArquivo}";
            using var con = new SqliteConnection(conexao);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS leituras (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                dispositivo_id TEXT NOT NULL,
                recebida_em INTEGER NOT NULL,
                capacitivo_adc INTEGER,
                resistivo_adc INTEGER,
                temperatura_c REAL,
                umidade_ar REAL,
                bomba_ligada INTEGER
            );
            CREATE INDEX IF NOT EXISTS ix_leituras_disp_tempo
                ON leituras (dispositivo_id, recebida_em);
            """;
            cmd.ExecuteNonQuery();
        }

        public void Salvar(DateTimeOffset recebidaEmUtc, LeituraSensores d)
        {
            using var con = new SqliteConnection(conexao);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = """
            INSERT INTO leituras
              (dispositivo_id, recebida_em, capacitivo_adc, resistivo_adc,
               temperatura_c, umidade_ar, bomba_ligada)
            VALUES ($d, $t, $c, $r, $temp, $u, $b);
            """;
            cmd.Parameters.AddWithValue("$d", d.DispositivoId ?? "");
            cmd.Parameters.AddWithValue("$t", recebidaEmUtc.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$c", (object?)d.CapacitivoAdc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$r", (object?)d.ResistivoAdc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$temp", (object?)d.TemperaturaC ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$u", (object?)d.UmidadeArPercentual ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$b", (object?)d.BombaLigada ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public IReadOnlyList<PontoHistorico> Consultar(
            string dispositivoId, DateTimeOffset de, DateTimeOffset ate, int janelaSegundos)
        {
            using var con = new SqliteConnection(conexao);
            con.Open();
            using var cmd = con.CreateCommand();
            
            cmd.CommandText = """
            SELECT (recebida_em / $j) * $j AS t,
                   AVG(capacitivo_adc), AVG(resistivo_adc),
                   AVG(temperatura_c),  AVG(umidade_ar)
            FROM leituras
            WHERE dispositivo_id = $d AND recebida_em BETWEEN $de AND $ate
            GROUP BY t ORDER BY t;
            """;
            cmd.Parameters.AddWithValue("$j", janelaSegundos);
            cmd.Parameters.AddWithValue("$d", dispositivoId);
            cmd.Parameters.AddWithValue("$de", de.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$ate", ate.ToUnixTimeSeconds());

            var lista = new List<PontoHistorico>();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                lista.Add(new PontoHistorico(
                    DateTimeOffset.FromUnixTimeSeconds(r.GetInt64(0)),
                    r.IsDBNull(1) ? null : r.GetDouble(1),
                    r.IsDBNull(2) ? null : r.GetDouble(2),
                    r.IsDBNull(3) ? null : r.GetDouble(3),
                    r.IsDBNull(4) ? null : r.GetDouble(4)));
            }
            return lista;
        }
    }
}
