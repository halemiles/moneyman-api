# Database Seeding Scripts

This directory contains scripts for initializing and seeding the Moneyman API database.

## Files

- **seed_database.py** - Python script to apply schema and seed data to SQLite database
- **files/schema.sql** - Database schema definition
- **files/seed-data.sql** - Initial seed data for the database

## Usage

### Automatic (on Application Boot)

The seed script runs automatically when the Moneyman API starts. It will:
1. Check if the database exists and has tables
2. If the database is empty, apply the schema and seed data
3. If the database already exists, skip seeding to preserve existing data

### Manual Execution

You can also run the seed script manually:

```bash
# From the Scripts directory
python3 seed_database.py

# Force re-seeding (will fail if data already exists due to unique constraints)
python3 seed_database.py --force
```

### Shell Script (Legacy)

The original shell script is still available:

```bash
# From the src directory
./Scripts/intitialise-db.sh
```

## Requirements

- Python 3.x
- sqlite3 module (included in Python standard library)

## How It Works

1. The Python script is called from `Program.cs` via the `DatabaseInitializer` class on application startup
2. The script checks if the database file exists and contains tables
3. If empty or missing, it applies:
   - `schema.sql` - Creates tables, indexes, and constraints
   - `seed-data.sql` - Inserts initial data for Paydays and Transactions
4. The script outputs progress information that is logged by the API

## Database Location

By default, the database is created at:
- Development: `src/Moneyman.Api/LocalDatabase.db`
- Docker: Volume-mapped location (see docker-compose.yml)

The location can be configured in `appsettings.json` under `ConnectionStrings:WebApiDatabase`.
