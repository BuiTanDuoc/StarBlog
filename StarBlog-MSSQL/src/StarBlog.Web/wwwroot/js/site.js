let app = new Vue({
    el: '#vue-header',
    data: {
        currentTheme: '',
        themes: []
    },
    created: function () {
        fetch('/Api/Theme')
            .then(res => res.json())
            .then(res => {
                this.themes = res.data
            })

        // Đọc cấu hình giao diện cục bộ
        let theme = localStorage.getItem('currentTheme')
        if (theme != null) this.currentTheme = theme
    },
    methods: {
        setTheme(themeName) {
            let theme = this.themes.find(t => t.name === themeName)
            loadStyles(theme.cssUrl)
            this.currentTheme = themeName
            localStorage.setItem('currentTheme', themeName)
            localStorage.setItem('currentThemeCssUrl', theme.cssUrl)
            // Sau khi đổi giao diện nên tải lại trang, nếu không có thể bị xung đột style
            location.reload()
        }
    }
})

let toastTrigger = document.getElementById('liveToastBtn')
let toastLiveExample = document.getElementById('liveToast')
if (toastTrigger) {
    toastTrigger.addEventListener('click', function () {
        let toast = new bootstrap.Toast(toastLiveExample)
        toast.show()
    })
}