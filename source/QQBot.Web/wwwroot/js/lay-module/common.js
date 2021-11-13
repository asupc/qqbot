layui.define(['jquery', 'layer'], function (exports) {
    var $ = layui.jquery;
    var layer = layui.layer;
    var obj = {
        ajax: function (url, type, data, callback,contentType) {
            $.ajax({
                url: "/api" + url,
                type: type,
                contentType:"application/json",
                data: data,
                beforeSend: function (request) {
                    request.setRequestHeader("Authorization", "Bearer " + layui.data("token").token);
                },
                success: function (data) {
                    if (data.Code == 200) {
                        callback(data.Data);
                    } else {
                        layer.msg(data.Message, { icon: 5 });
                    }
                },
                error: function (e) {
                    if (e.status == 401) {
                        window.top.location = "/login.html"
                    }
                    console.log(url + "Rquest Error：" + JSON.stringify(e));
                }
            });
        },
        queryString: function (name) {
            var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i');
            var r = window.location.search.substr(1).match(reg);
            if (r != null) {
                return unescape(r[2]);
            }
            return null;
        }
    }
    //输出接口
    exports('common', obj);
});