## 特别声明: 

* 本仓库发布的项目中涉及的任何解锁和解密分析脚本，仅用于测试和学习研究，禁止用于商业用途，不能保证其合法性，准确性，完整性和有效性，请根据情况自行判断.

* 本项目内所有资源文件，禁止任何公众号、自媒体进行任何形式的转载、发布。

* 本仓库拥有者对任何脚本问题概不负责，包括但不限于由任何脚本错误导致的任何损失或损害.

* 间接使用脚本的任何用户，包括但不限于建立VPS或在某些行为违反国家/地区法律或相关法规的情况下进行传播, 本仓库拥有者对于由此引起的任何隐私泄漏或其他后果概不负责.

* 请勿将项目的任何内容用于商业或非法目的，否则后果自负.

* 如果任何单位或个人认为该项目的脚本可能涉嫌侵犯其权利，则应及时通知并提供身份证明，所有权证明，我们将在收到认证文件后删除相关脚本.

* 任何以任何方式查看此项目的人或直接或间接使用该MyActions项目的任何脚本的使用者都应仔细阅读此声明。本仓库拥有者保留随时更改或补充此免责声明的权利。一旦使用并复制了任何相关脚本或MyActions项目的规则，则视为您已接受此免责声明.

 **您必须在下载后的24小时内从计算机或手机中完全删除以上内容.**  </br>
 ***您使用或者复制了本仓库且本人制作的任何脚本，则视为`已接受`此声明，请仔细阅读*** 

** 该程序一切功能测试环境为centos8，青龙面板版本2.9。 不保证其他环境功能正常 **

## 即日起，本仓库仅用于个人学习使用，如若想详细了解，请往下看。


## go环境安装 已安装的请忽略
 ```
sudo -i ##root权限
cd /usr/local
wget https://golang.google.cn/dl/go1.16.7.linux-amd64.tar.gz  ##local目录下载
tar -xvzf go1.16.7.linux-amd64.tar.gz ##解压
vi /etc/profile ##打开文件，设置环境变量，输入“i”进行编辑文件 或者使用用FinalShell 打开 /etc/profile文件。复制以下内存到文件末尾处。


export GO111MODULE=on
export GOPROXY=https://goproxy.cn
export GOROOT=/usr/local/go
export GOPATH=/usr/local/go/path
export PATH=$PATH:$GOROOT/bin:$GOPATH/bin


## 保存并退出。
source /etc/profile
go env #运行后，查看变量一样就设置对了。

 ```

 ## 安装 node 环境 已安装的请忽略
 ```
sudo -i ##root权限

yum install -y epel-release
/usr/bin/yum install -y nodejs

执行 node ，不报错则安装完成

 ```

 ## 安装 .net core 运行环境 已安装的请忽略
 ```
sudo -i ##root权限

sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm  ##注册 Microsoft 密钥和源
sudo yum install dotnet-sdk-3.1  ##安装 .NET Core SDK
sudo yum install aspnetcore-runtime-3.1 ##安装 ASP.NET Core 运行时
dotnet –version ##测试.NET SDK 是否正常。

 ```


 ## 拉取QQBot 运行程序
 ```
 yum install git  ## 安装git
 cd /root  ## 程序存放目录自定义。
 git clone https://github.com/asupc/qqbot.git ## 克隆程序到本地。

 ```

 ## 运行 go-cqhttp 本库自带一个编译好的未修改任何代码 go-cqhttp
 附官方帮助文档：https://docs.go-cqhttp.org/
 github：https://github.com/Mrs4s/go-cqhttp
 ```
 cd /root/qqbot  ## 切换到程序目录。
 chmod 777 go-cqhttp ## 给权限
./go-cqhttp ## 先运行一遍，等控制台输出二维码用一个QQ号扫码登录作为机器人。登录后 ctrl+c 结束。
nohup ./go-cqhttp   ## 后台运行

 ## 修改机器人配置，配置文件在 bot/appsettings.json 请打开文件参考修改
 cd bot/Scripts
 npm install ## 安装js依赖
 cd ..
 nohup dotnet QQBot.Web.dll &  ## 后台运行机器人主程序。
 ```


 ## QQBot 配置文件
```
 {
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "GoCQHttp": {
    "Http": "http://localhost:8000",   // go-cqhttp http 默认不修改
    "WebSocket": "ws://localhost:8001", // go-cqhttp websocket 默认不修改
    "Groups": [ 124567958 ], // 监控的QQ群号，多个用逗号隔开。
    "Manager": 179100150  // 管理QQ号，暂时无特殊作用。
  },
  "SystemConfig": {
    "ua": "Mozilla/5.0 (iPhone; CPU iPhone OS 13_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 SP-engine/2.14.0 main%2F1.0 baiduboxapp/11.18.0.16 (Baidu; P2 13.3.1) NABar/0.0",
    "agree_friend": true, // 是否自动通过好友验证
    "agree_group": true, // 是否自动通过加群验证 (暂时没用)
    "notify": {
      "DDNC": true, // 是否开启东东农场通知(暂时没用)
      "DDGC": true, // 是否开启东东工厂通知(暂时没用)
      "JXNC": true, // 是否开启惊喜农场通知(暂时没用)
      "JXGC": true // 是否开启惊喜工厂通知(暂时没用)
    }
  },
  "QLConfig": [
    {
      "Address": "http://localhost:5700", //青龙地址
      "UserName": "admin", // 青龙用户名
      "PassWord": "admin", // 青龙面板密码
    }
  ],
  "DBConfig": {
    "DBType": "SQLite", // 数据库类型， 可选 SQLite 和 MySQL
    "Address": "QQBot.db" // 数据库地址： 数据库类型为SQLite 时，填写文件名，为MySQL 时填写数据库链接，如：server=192.168.0.1;port=3306;Database=dbname;Uid=root;Pwd=123456;CharSet=utf8
  },
  // 自定义回复内容。
  "Commands": [
    {
      "Key": "公告",
      "Message": "本机器人所管理青龙面板所执行脚本均来自于“https://github.com/shufflewzc/faker2.git”\r\n 默认会清空购物车，取消店铺关注，商品关注，主播关注，请知晓。"
    },
    {
      "Key": "活动",
      "Message": "玩一玩（可找到大多数活动)∶\n京东APP首页-频道-边玩边赚\n宠汪汪:京东APP-首页/玩一玩/我的-宠汪汪\n东东萌宠:京东APP-首页/玩一玩/我的-东东萌宠\n东东农场:京东APP-首页/玩一玩/我的-东东农场\n东东工厂:京东APP-首页/玩一玩/我的-东东工厂\n东东超市:京东APP-首页/玩一玩/我的-东东超市\n领现金:京东APP-首页/玩一玩/我的-领现金\n东东健康社区:京东APP-首页/玩一玩/我的-东东健康社区\n京喜农场:京喜APP-我的-京喜农场\n京喜牧场:京喜APP-我的-京喜牧场\n京喜工厂:京喜APP-我的-京喜工厂\n京喜财富岛:京喜APP-我的-京喜财富岛\n京东极速版红包:京东极速版APP-我的-红包"
    },
    {
      "Key": "教程",
      "Message": "请复制网页地址到手机或电脑浏览器打开，\n安卓手机获取Cookie：\n“https://docs.qq.com/doc/DR1NTUkJObXFGeGpr”\n电脑Cookie获取\n“https://docs.qq.com/doc/DR1NTUkJObXFGeGpr”\nCookie 格式化提交地址：\nhttp://nas.678120.cn:5701/#/login\n如需要其他帮助请联系QQ：179100150`"
    }
  ],
  "Quartzs": {
    "CheckCookieJob": "0 0 * * * ? *",  // 检查Cookie Cron表达式
    "QueryJob": "0 0 12 * * ? *"    // 定时推送账号信息Cron表达式
  }
}
```