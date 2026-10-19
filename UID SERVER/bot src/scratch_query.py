import sqlite3
try:
    c = sqlite3.connect('/root/bypass_project/bot_data.db')
    cur = c.cursor()
    cur.execute("SELECT name FROM sqlite_master WHERE type='table'")
    tables = cur.fetchall()
    print("Tables:", tables)
    for table_name in tables:
        t = table_name[0]
        cur.execute(f"SELECT COUNT(*) FROM {t}")
        cnt = cur.fetchone()[0]
        print(f"Table {t}: count = {cnt}")
        if cnt > 0:
            cur.execute(f"SELECT * FROM {t} LIMIT 5")
            print(f"Sample from {t}:", cur.fetchall())
except Exception as e:
    print(f"Error: {e}")
