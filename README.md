To run database
```
docker run --name krabby-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=krabby \
  -p 5432:5432 \
  -d postgres
```

```
docker run -d \
  --name krabby-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=krabby \
  -p 5432:5432 \
  postgres:15
```

\dt should show
```
fjura@DESKTOP-IB10381:~$ docker exec -it krabby-db psql -U postgres -d krabby
psql (18.3 (Debian 18.3-1.pgdg13+1))
Type "help" for help.

krabby=# \dt
                  List of tables
 Schema |         Name          | Type  |  Owner
--------+-----------------------+-------+----------
 public | Anime                 | table | postgres
 public | Episodes              | table | postgres
 public | __EFMigrationsHistory | table | postgres
(3 rows)

krabby=#
```
```
krabby=# select count(*) from "Anime";
 count
-------
     0
(1 row)

krabby=#
```

2026-05-03T03:30:08.5535719-07:00
2026-05-03T04:04:34.9194042-07:00
"LastLogin": "2026-05-03T11:46:30.6350555-07:00",

```
{
  "status": "done",
  "data": {
    "animeName": "Keijo!!!!!!!!",
    "airDateYear": "2016",
    "session_key": "wIelR",
    "aid": 11922,
    "episodeData": [
      {
        "eid": 180393,
        "episodeNumber": "01",
        "type": "Regular"
      },
      {
        "eid": 180392,
        "episodeNumber": "02",
        "type": "Regular"
      },
      {
        "eid": 180391,
        "episodeNumber": "03",
        "type": "Regular"
      },
      {
        "eid": 180390,
        "episodeNumber": "04",
        "type": "Regular"
      },
      {
        "eid": 181345,
        "episodeNumber": "05",
        "type": "Regular"
      },
      {
        "eid": 181344,
        "episodeNumber": "06",
        "type": "Regular"
      },
      {
        "eid": 181343,
        "episodeNumber": "07",
        "type": "Regular"
      },
      {
        "eid": 181342,
        "episodeNumber": "08",
        "type": "Regular"
      },
      {
        "eid": 182231,
        "episodeNumber": "09",
        "type": "Regular"
      },
      {
        "eid": 182230,
        "episodeNumber": "10",
        "type": "Regular"
      },
      {
        "eid": 182229,
        "episodeNumber": "11",
        "type": "Regular"
      },
      {
        "eid": 182228,
        "episodeNumber": "12",
        "type": "Regular"
      }
    ]
  }
}
```