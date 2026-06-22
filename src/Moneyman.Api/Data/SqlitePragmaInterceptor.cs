using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Moneyman.Api.Data
{
    /// <summary>
    /// Runs SQLite PRAGMAs on every opened connection so the API tolerates
    /// concurrent access. busy_timeout makes a writer wait for a held lock
    /// (rather than failing immediately with "database is locked"), and WAL lets
    /// readers run alongside the single writer. Without this, parallel callers
    /// (e.g. the Playwright suite's workers) collide on SQLite's single writer
    /// and POSTs intermittently fail.
    /// </summary>
    public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
    {
        private const string PragmaSql = "PRAGMA busy_timeout=5000; PRAGMA journal_mode=WAL;";

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            using var command = connection.CreateCommand();
            command.CommandText = PragmaSql;
            command.ExecuteNonQuery();
        }

        public override async Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = PragmaSql;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
