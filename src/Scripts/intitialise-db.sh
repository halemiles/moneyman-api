#!/bin/bash

echo "Creating database file"
touch Moneyman.Api/LocalDatabase.db

echo "Importing sqlite DB"
sqlite3 ./Moneyman.Api/LocalDatabase.db < ./Scripts/files/schema.sql

echo "Importing seed data"
sqlite3 ./Moneyman.Api/LocalDatabase.db < ./Scripts/files/seed-data.sql