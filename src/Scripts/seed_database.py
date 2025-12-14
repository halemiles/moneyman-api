#!/usr/bin/env python3
"""
Database seeding script for Moneyman API.
This script applies schema and seed data to the SQLite database on application boot.
"""

import sqlite3
import os
import sys
from pathlib import Path


def get_database_path():
    """Get the database path from the configuration or use default."""
    # Default path relative to the API project
    return os.path.join(
        os.path.dirname(__file__), 
        "..", 
        "Moneyman.Api", 
        "LocalDatabase.db"
    )


def get_sql_file_path(filename):
    """Get the path to SQL files."""
    return os.path.join(
        os.path.dirname(__file__),
        "files",
        filename
    )


def database_exists(db_path):
    """Check if database file exists."""
    return os.path.exists(db_path)


def is_database_empty(db_path):
    """Check if the database has any tables."""
    if not database_exists(db_path):
        return True
    
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'")
        tables = cursor.fetchall()
        conn.close()
        return len(tables) == 0
    except Exception as e:
        print(f"Error checking database: {e}")
        return True


def execute_sql_file(db_path, sql_file_path):
    """Execute SQL statements from a file."""
    if not os.path.exists(sql_file_path):
        print(f"Warning: SQL file not found: {sql_file_path}")
        return False
    
    try:
        with open(sql_file_path, 'r') as sql_file:
            sql_script = sql_file.read()
        
        conn = sqlite3.connect(db_path)
        # Set isolation_level to None to allow executescript to handle transactions.
        # The SQL files contain BEGIN TRANSACTION/COMMIT statements that need autocommit mode.
        # executescript() automatically commits before executing, so this is safe.
        conn.isolation_level = None
        cursor = conn.cursor()
        cursor.executescript(sql_script)
        conn.close()
        return True
    except Exception as e:
        print(f"Error executing SQL file {sql_file_path}: {e}")
        return False


def seed_database(force_seed=False):
    """
    Initialize and seed the database.
    
    Args:
        force_seed: If True, seed data even if database already exists
    """
    db_path = get_database_path()
    schema_path = get_sql_file_path("schema.sql")
    seed_data_path = get_sql_file_path("seed-data.sql")
    
    print("=" * 50)
    print("Moneyman API - Database Seeding")
    print("=" * 50)
    print(f"Database path: {db_path}")
    
    # Ensure the directory exists
    db_dir = os.path.dirname(db_path)
    if not os.path.exists(db_dir):
        os.makedirs(db_dir, exist_ok=True)
        print(f"Created database directory: {db_dir}")
    
    # Check if database needs initialization
    needs_schema = is_database_empty(db_path)
    
    if needs_schema:
        print("Database is empty or doesn't exist. Applying schema...")
        if execute_sql_file(db_path, schema_path):
            print("✓ Schema applied successfully")
        else:
            print("✗ Failed to apply schema")
            return False
    else:
        print("Database already exists with schema")
    
    # Apply seed data if database was just created or force_seed is True
    if needs_schema or force_seed:
        print("Applying seed data...")
        if execute_sql_file(db_path, seed_data_path):
            print("✓ Seed data applied successfully")
        else:
            print("✗ Failed to apply seed data")
            return False
    else:
        print("Skipping seed data (database already initialized)")
    
    print("=" * 50)
    print("Database initialization complete")
    print("=" * 50)
    return True


if __name__ == "__main__":
    # Check for command line arguments
    force_seed = "--force" in sys.argv or "-f" in sys.argv
    
    success = seed_database(force_seed=force_seed)
    sys.exit(0 if success else 1)
