lighthouse run -what ping -where 127.0.0.1
lighthouse inspect -where 127.0.0.1 

lighthouse install -what backup_server -where 127.0.0.1 -how "{ updateTime: }"
lighthouse benchmark -what latency -who 127.0.0.1