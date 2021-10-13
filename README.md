
## 已提供 linux-x64，windows-x64，docker 版本
## 各版本库自取。
## 打包文件包含.net core 3.1 运行环境。linux 系统 ./QQBot.Web 直接运行。windows 直接双击QQBot.Web.exe 运行。

### 一般docker用户参考 docker-compose.yaml
### 群晖用户参考 asupc-qqbot.json
### 推荐使用docker-compose 启动，使用sqlite 注意挂载 db，config 文件夹
### config/InstallConfig.xml为配置文件，配置错误无法启动请删除该文件后重新初始化。


## 实现功能
```
1. 账号管理：通过账号管理可以管理已有的Cookie，编辑权重，置顶Cookie，编辑绑定的QQ号码，批量启用，禁用，删除Cookie。同步Cookie 到青龙面板
2. 通过QQ发送Cookie给机器人，自动绑定关系 使用格式 pt_key=xx;pt_pin=xxx;qq=xxx; 可以绑定到指定的QQ
3. 青龙面板管理：支持Client ID, Secret认证方式登陆，支持面板全量模式（全量模式的面板将会同步所有cookie 到青龙容器。）
4. 快捷回复：通过网页配置 指令和回复内容。
5. 系统设置：通过页面配置go-cqhttp 配置，配置监管QQ群和管理QQ号。
6. 脚本指令：自定义上传脚本配置执行指令来实现查询，月度查询等功能，允许脚本并发。
7. 环境变量：为脚本提供环境变量信息。
8. 数据导入：支持导入xdd 数据库数据，还原cookie 和 QQ绑定关系。
9. 系统指令：
    更新QQBot：内置自动更新功能。
    清理过期Cookie：自动清理已过期的Cookie
    检查Cookie：检查Cookie是否过期（系统会定时检查Cookie是否过期，并推送消息给QQ好友）
    同步Cookie：将QQBot的Cookie 同步到青龙面板。
    消息推送：批量推送自定义消息给用户格式（消息推送 这是推送的消息），注意空格
    统计Cookie：统计机器人管理的Cookie信息。
    到处Cookie：将Cookie导出到文本中，包含青龙面板的json格式，和QQBot支持的 pt_key=xx;pt_pin=xxx;qq=xxx;
10. 其他： 自动处理好友请求，群聊Cookie 自动撤回（机器人需要设置为管理员），脚本结果均以私聊的形式发送，不在群聊回复脚本结果。
```


### docker run 运行 

```
 docker run --name qqbot1 -v /root/qqbot1/db:/app/linux-x64/db -v /root/qqbot1/config:/app/linux-x64/config -v /root/qqbot1/scripts:/app/linux-x64/scripts -p 5010:5010 asupc/qqbot -restart:always
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
 docker run --name qqbot1 -v /root/qqbot1/db:/app/linux-x64/db -v /root/qqbot1/config:/app/linux-x64/config -v /root/qqbot1/scripts:/app/linux-x64/scripts -p 5010:5010 asupc/qqbot -restart:always
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
