---
一般docker用户参考 docker-compose.yaml
群晖用户参考 asupc-qqbot.json
---
## 直接克隆本仓库地址。

```
yum install -y git ## 安装git

git clone https://github.com/asupc/qqbot.git /roo/qqbot1 ## 克隆仓库

cd /roo/qqbot1 && docker-compose up -d ## 启动qqbot

```

## 浏览器输入地址：

http://ip:5010/login.html 自己初始化设置用户名密码，及数据库。初始化后请重启容器。

```
docker restart qqbot
```
