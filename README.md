
## 一般docker用户参考 docker-compose.yaml
## 群晖用户参考 asupc-qqbot.json
## 推荐使用docker-compose 启动，使用sqlite 注意挂载 db 文件夹
## /app/config/InstallConfig.xml为配置文件，配置错误无法启动请删除该文件后重新初始化。

## 如无docker-compose 

```
docker run --name qqbot1 -v /root/qqbot1/db:/app/db -v /root/qqbot1/config:/app/config -p 5010:5010 asupc/qqbot -restart:always
```

---
更新镜像
---

```
docker pull asupc/qqbot
```

 ---
 docker-compose 更新
 ---
 ```
 docker-compose up -d
 ```
 
 ---
 docker run 更新
 ---
 ```
 docker stop qqbot1
 docker rm qqbot1
 docker run --name qqbot1 -v /root/qqbot1/db:/app/db -v /root/qqbot1/config:/app/config -p 5010:5010 asupc/qqbot -restart:always
 ```

 ---
 docker 重启
 ---
 
 ```
 docker restart qqbot1
 ```

## docker 地址
```
https://hub.docker.com/r/asupc/qqbot

```
