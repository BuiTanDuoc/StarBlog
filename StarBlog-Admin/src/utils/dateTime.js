Date.prototype.format = function (fmt) {
  let o = {
    "M+": this.getMonth() + 1,                 //Tháng
    "d+": this.getDate(),                    //Ngày
    "h+": this.getHours(),                   //Giờ
    "m+": this.getMinutes(),                 //Phút
    "s+": this.getSeconds(),                 //Giây
    "q+": Math.floor((this.getMonth() + 3) / 3), //Quý
    "S": this.getMilliseconds()             //Mili giây
  }
  if (/(y+)/.test(fmt)) {
    fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length))
  }
  for (let k in o) {
    if (new RegExp("(" + k + ")").test(fmt)) {
      fmt = fmt.replace(RegExp.$1, (RegExp.$1.length === 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)))
    }
  }
  return fmt
}

export const dateTimeBeautify = function (dateTimeStr) {
  let dateObj = new Date(dateTimeStr)
  return dateObj.format("yyyy-MM-dd hh:mm:ss")
}
