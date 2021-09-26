
## 已提供linux-arm ，linux-x64，windows-x64，osx，windows arm，docker 版本
## 各版本库自取。
## windows x64 linux x64 docker 测试正常，arm架构请自行测试。
## 打包文件包含.net core 3.1 运行环境。linux 系统 ./QQBot.Web 直接运行。windows 直接双击QQBot.Web.exe 运行。

### 一般docker用户参考 docker-compose.yaml
### 群晖用户参考 asupc-qqbot.json
### 推荐使用docker-compose 启动，使用sqlite 注意挂载 db，config 文件夹
### config/InstallConfig.xml为配置文件，配置错误无法启动请删除该文件后重新初始化。


### docker run 运行 

```
docker run --name qqbot1 -v /root/qqbot1/db:/app/db -v /root/qqbot1/config:/app/config -p 5010:5010 asupc/qqbot -restart:always
```

---
### 更新镜像
---

```
docker pull asupc/qqbot
```

 ---
 ### docker-compose 更新
 ---
 ```
 docker stop qqbot1
 docker rm qqbot1
 docker rmi asupc/qqbot
 docker-compose up -d
 ```
 
 ---
 ### docker run 更新
 ---
 ```
 docker stop qqbot1
 docker rm qqbot1
 docker rmi asupc/qqbot
 docker run --name qqbot1 -v /root/qqbot1/db:/app/linux-x64/db -v /root/qqbot1/config:/app/linux-x64/config -p 5010:5010 asupc/qqbot -restart:always
 ```

 ---
 ### docker 重启
 ---
 
 ```
 docker restart qqbot1
 ```

### docker 地址
```
https://hub.docker.com/r/asupc/qqbot

```
