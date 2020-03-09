/**
 * 创建文件夹
 * @param {any} curDirectoryId 当前目录id
 */
function createDirectory(curDirectoryId) {
    var date = new Date();
    console.log(date);
    var name = "新建文件夹";
    Swal.fire({
        title: '输入新建文件夹名称',
        confirmButtonText: "确认",
        cancelButtonText: '取消',
        input: 'text',
        inputValue: name,
        showCancelButton: true,
        inputValidator: function (value){
            if (!value) {
                return '无效的文件名';
            }
        }
    }).then(function (value) {
        if (value && value.value !== "" && value.value !== undefined) {
            axios.post("/Index?handler=CreateDirectory", { PId: curDirectoryId, Name: value.value }).then(function (res) {
                window.location.reload();
            });
        }
    });
}

/**
 * 删除文件夹
 * @param {any} directoryId 目录id
 * @param {any} directoryName 目录名称
 */
function deleteDirectory(directoryId, directoryName) {
    Swal.fire({
        confirmButtonText: "确认",
        cancelButtonText: '取消',
        title: '确认删除目录【' + directoryName + "】吗？可以在回收站中找回删除的文件夹",
        showCancelButton: true
    }).then(function (val) {
        if (val.value) {
            axios.post("/Index?handler=DeleteDirectory&directoryId=" + directoryId).then(function (res) {
                window.location.reload();
            });
        }
    });
}

/**
 * 删除文件
 * @param {any} fileId 文件id
 * @param {any} fileName 文件名称
 */
function deleteFile(fileId, fileName) {
    Swal.fire({
        confirmButtonText: "确认",
        cancelButtonText: '取消',
        title: '确认删除文件【' + fileName + "】吗？可以在回收站中找回删除的文件",
        showCancelButton: true
    }).then(function (val) {
        if (val.value) {
            axios.post("/Index?handler=DeleteFile&fileId=" + fileId).then(function (res) {
                window.location.reload();
            });
        }
    });
}

/**
 * 重命名文件
 * @param {any} fileId 文件id
 * @param {any} oldFileName 旧的文件名
 */
function renameFile(fileId, oldFileName) {
    //重命名Rename
    Swal.fire({
        title: '输入文件名',
        input: 'text',
        inputValue: oldFileName,
        confirmButtonText: "确认",
        cancelButtonText: '取消',
        showCancelButton: true,
        inputValidator: function (value){
            if (!value) {
                return '无效的文件名';
            }
        }
    }).then(function (value) {
        if (value && value.value !== "" && value.value !== undefined) {
            axios.post("/Index?handler=Rename&fileId=" + fileId + "&newName=" + value.value).then(function (res) {
                window.location.reload();
            });
        }
    });
}


/**
 * 重命名文件夹
 * @param {any} directoryId 文件夹id
 * @param {any} oldName 旧的文件夹名称
 */
function renameDirectory(directoryId, oldName) {
    Swal.fire({
        title: '输入文件名',
        input: 'text',
        inputValue: oldName,
        showCancelButton: true,
        confirmButtonText: "确认",
        cancelButtonText: '取消',
        inputValidator: function (value){
            if (!value) {
                return '无效的名称';
            }
        }
    }).then(function (value) {
        if (value && value.value !== "" && value.value !== undefined) {
            axios.post("/Index?handler=RenameDirectory", {
                DirectoryId: directoryId,
                NewName: value.value
            }).then(function (res) {
                window.location.reload();
            });
        }
    });
}


function conver(limit) {
    var size = "";
    if (limit < 0.1 * 1024) { //如果小于0.1KB转化成B
        size = limit.toFixed(2) + "B";
    } else if (limit < 0.1 * 1024 * 1024) {//如果小于0.1MB转化成KB
        size = (limit / 1024).toFixed(2) + "KB";
    } else if (limit < 0.1 * 1024 * 1024 * 1024) { //如果小于0.1GB转化成MB
        size = (limit / (1024 * 1024)).toFixed(2) + "MB";
    } else { //其他转化成GB
        size = (limit / (1024 * 1024 * 1024)).toFixed(2) + "GB";
    }

    var sizestr = size + "";
    var len = sizestr.indexOf("\.");
    var dec = sizestr.substr(len + 1, 2);
    if (dec == "00") {//当小数点后为00时 去掉小数部分
        return sizestr.substring(0, len) + sizestr.substr(len + 3, 2);
    }
    return sizestr;
}


/**
 * 获取文件属性
 * @param {any} fileId 文件id
 */
function getFileInfoDlg(fileId) {
    axios.get("/Index?handler=FileInfo&fileId=" + fileId).then(function (res) {
        var file = res.data;

        Swal.fire({
            confirmButtonText: "确认",
            title: '<strong>' + file.originFileName + '</strong>',
            html: '<div class="text-left"><p>大小： ' + conver(file.size) + '</p>' +
                '<p>创建时间： ' + file.createTime + '</p>' +
                '<p>修改时间： ' + file.modifyDatetime + '</p></div>'
        });
        console.log(file);
    });
}

/**
 * 从回收站恢复目录
 * @param {any} directoryId 目录id
 */
function recoveryDirectory(directoryId) {
    axios.post("/Recyclebin?handler=RecoveryDirectory&directoryId=" + directoryId, {}).then(function (res) {
        window.location.reload();
    });
}

/**
 * 从回收站恢复文件
 * @param {any} fileId 文件id
 */
function recoveryFile(fileId) {
    axios.post("/Recyclebin?handler=RecoveryFile&fileId=" + fileId, {}).then(function (res) {
        window.location.reload();
    });
}


function downloadFile(fileId) {
    window.open('/Index?handler=DownloadFile&fileId=' + fileId);
}


function sharedFile(fileId) {
    axios.get("/Index?handler=FileInfo&fileId=" + fileId).then(function (res) {
        var file = res.data;

        Swal.fire({
            confirmButtonText: "创建分享",
            title: '分享文件：' + file.originFileName,
            html:
                '<div class="text-left">' +
                '<hr/>' +
                '<label>分享形式：</label><br/>' +
                '<input type="radio" style="margin-left:80px" name="swal-input1" id="hadcode" value="1" checked/><label for="hadcode">有提取码(仅限有提取码的用户查看)</label><br/>' +
                '<input type="radio" style="margin-left:80px" name="swal-input1" id="nocode" value="0"/><label for="nocode">无提取码(任何人都可以查看)</label>' +
                '<hr/>' +
                '<label>有效期：</label><br/>' +
                '<input type="radio" style="margin-left:80px" name="swal-input2" id="endDatetime1" value="-1" checked/><label for="endDatetime1">永久有效</label><br/>' +
                '<input type="radio" style="margin-left:80px" name="swal-input2" id="endDatetime2" value="7"/><label for="endDatetime2">7天</label><br/>' +
                '<input type="radio" style="margin-left:80px" name="swal-input2" id="endDatetime3" value="1"/><label for="endDatetime3">1天</label><br/>' +
                '</div>',

            focusConfirm: false,
            preConfirm: function (){
                return [
                    $('input[name="swal-input1"]:checked').val(),
                    $('input[name="swal-input2"]:checked').val()
                ];
            }
        }).then(function (value) {
            if (value) {
                console.log(value.value);
                axios.post('/Index?handler=CreateShared', {
                    FileId: fileId,
                    HadCode: value.value[0] === "1",
                    Days: parseInt(value.value[1])
                }).then(function (res) {
                    console.log(res.data);

                    Swal.fire({
                        confirmButtonText: "OK",
                        //title: '分享文件：' + file.originFileName,
                        html: '<div class="text-left">' +
                            '创建成功' +
                            '<p>创建时间： ' + file.createTime + '</p>' +
                            '<p>修改时间： ' + file.modifyDatetime + '</p></div>'
                    });
                });
            }
        });

        //Swal.fire({
        //    confirmButtonText: "确认",
        //    title: '分享文件：' + file.originFileName,
        //    html: '<div class="text-left">' +
        //        '<input/>' +
        //        '<p>创建时间： ' + file.createTime + '</p>' +
        //        '<p>修改时间： ' + file.modifyDatetime + '</p></div>'
        //});
        //console.log(file);
    });
}