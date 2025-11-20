import argparse
import requests
from rich.console import Console
from rich.table import Table
from datetime import datetime

# Replace with your actual API base URL
API_URL = "http://localhost:5000/Dtp/current"

def fetch_current_period(period="current", starting_value=None, bank_account_id=None):
    # Build endpoint based on requested period ('current' or 'full')
    api_url = f"http://localhost:5000/Dtp/{period}"

    params = {}
    if starting_value is not None:
        params['startingValue'] = starting_value
    if bank_account_id is not None:
        params['bankAccountId'] = bank_account_id

    response = requests.get(api_url, params=params)
    response.raise_for_status()
    return response.json()

def _parse_date_str(s):
    if not isinstance(s, str):
        return None
    # try ISO formats first (handle trailing Z)
    try:
        iso = s
        if iso.endswith('Z'):
            iso = iso[:-1] + '+00:00'
        return datetime.fromisoformat(iso)
    except Exception:
        pass
    # fallback common patterns
    patterns = [
        "%Y-%m-%dT%H:%M:%S.%fZ",
        "%Y-%m-%dT%H:%M:%SZ",
        "%Y-%m-%dT%H:%M:%S",
        "%Y-%m-%d",
        "%d/%m/%Y",
        "%d-%m-%Y",
        "%m/%d/%Y",
    ]
    for p in patterns:
        try:
            return datetime.strptime(s, p)
        except Exception:
            continue
    return None

def display_table(data):
    console = Console()
    table = Table(title="Current DTP Period")

    # If data is a list, use the first element to determine columns
    if isinstance(data, list):
        if not data:
            console.print("No data to display")
            return
        first = data[0]
        if isinstance(first, dict):
            keys = list(first.keys())
        else:
            # For non-dict list items, show a single column with their string representation
            table.add_column("value", style="cyan")
            for item in data:
                table.add_row(str(item))
            console.print(table)
            return
    elif isinstance(data, dict):
        keys = list(data.keys())
        data = [data]  # normalize to list of rows
    else:
        # Fallback for unexpected types
        table.add_column("value", style="cyan")
        table.add_row(str(data))
        console.print(table)
        return

    # detect a date-like column (first key containing 'date')
    date_key = None
    for k in keys:
        if 'date' in k.lower():
            date_key = k
            break

    # If we have a date column, sort rows by parsed date (missing -> end)
    if date_key:
        def _sort_key(row):
            if not isinstance(row, dict):
                return datetime.max
            parsed = _parse_date_str(row.get(date_key))
            return parsed if parsed is not None else datetime.max
        data = sorted(data, key=_sort_key)

    # Add columns and rows for dict-based data
    for key in keys:
        table.add_column(key, style="cyan")

    for row in data:
        # If an item is not a dict, convert it to an empty dict so .get works
        if not isinstance(row, dict):
            row = {}
        cells = []
        for k in keys:
            v = row.get(k, "")
            if k == date_key:
                parsed = _parse_date_str(v)
                if parsed:
                    cells.append(parsed.strftime("%d %b"))  # e.g. "01 May"
                else:
                    cells.append(str(v))
            else:
                cells.append(str(v))
        table.add_row(*cells)

    console.print(table)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Fetch and display DTP period data")
    parser.add_argument("--period", choices=["current", "full"], default="current",
                        help="Which period endpoint to call (default: current)")
    parser.add_argument("--starting-value", type=float, default=None, help="Optional startingValue param")
    parser.add_argument("--bank-account-id", type=str, default=None, help="Optional bankAccountId param")
    args = parser.parse_args()

    result = fetch_current_period(period=args.period,
                                  starting_value=args.starting_value,
                                  bank_account_id=args.bank_account_id)
    # adapt to existing response shape
    payload = result.get('payload', {})
    plan_dates = payload.get('planDates', payload)  # fallback if payload shape differs
    display_table(plan_dates)