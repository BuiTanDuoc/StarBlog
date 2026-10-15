/// <binding BeforeBuild='min' Clean='clean' ProjectOpened='auto' />
"use strict";

//Nạp các plugin gulp được sử dụng
const gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-clean-css"),
    rename = require("gulp-rename"),
    uglify = require("gulp-uglify"),
    changed = require("gulp-changed"),
    obfuscate = require('gulp-obfuscate');

//Định nghĩa đường dẫn lưu các tệp trong wwwroot
const paths = {
    root: "./wwwroot/",
    css: './wwwroot/css/',
    js: './wwwroot/js/',
    lib: './wwwroot/lib/'
};

//css
paths.cssDist = paths.css + "**/*.css";//Khớp đường dẫn của tất cả tệp css
paths.minCssDist = paths.css + "**/*.min.css";//Khớp đường dẫn của tất cả tệp css sau khi nén tương ứng
paths.concatCssDist = paths.css + "app.min.css";//Đường dẫn sau khi nén tất cả css vào một tệp css

//js
paths.jsDist = paths.js + "**/*.js";//Khớp đường dẫn của tất cả tệp js
paths.minJsDist = paths.js + "**/*.min.js";//Khớp đường dẫn của tất cả tệp js sau khi nén tương ứng
paths.concatJsDist = paths.js + "app.min.js";//Đường dẫn sau khi nén tất cả js vào một tệp js


//Các gói thành phần frontend tải về bằng npm
const libs = [
    {name: "axios", dist: "./node_modules/axios/dist/**/*.*"},
    {name: "chart.js", dist: "./node_modules/chart.js/dist/**/*.*"},
    {name: "jquery", dist: "./node_modules/jquery/dist/**/*.*"},
    {name: "popper", dist: "./node_modules/popper.js/dist/**/*.*"},
    {name: "bootstrap", dist: "./node_modules/bootstrap/dist/**/*.*"},
    {name: "bootstrap5-treeview", dist: "./node_modules/bootstrap5-treeview/dist/**/*.*"},
    {name: "bootswatch", dist: "./node_modules/bootswatch/dist/**/*.*"},
    {name: "prismjs", dist: "./node_modules/prismjs/**/*.*"},
    {name: 'vue', dist: './node_modules/vue/dist/**/*.*'},
    {name: 'masonry-layout', dist: './node_modules/masonry-layout/dist/*.*'},
    // {name: 'echarts', dist: './node_modules/echarts/dist/**/*.*'},
    // {name: 'echarts-wordcloud', dist: './node_modules/echarts-wordcloud/dist/**/*.*'},
];

// Các thành phần frontend tải về bằng npm, tùy chỉnh vị trí lưu
const customLibs = [
    {name: "dayjs", dist: "./node_modules/dayjs/**/*.*"},
    {name: "editormd", dist: "./node_modules/editor.md-ext/*.js"},
    {name: "editormd/css", dist: "./node_modules/editor.md-ext/css/*.*"},
    {name: "editormd/fonts", dist: "./node_modules/editor.md-ext/fonts/*.*"},
    {name: "editormd/images", dist: "./node_modules/editor.md-ext/images/*.*"},
    {name: "editormd/languages", dist: "./node_modules/editor.md-ext/languages/*.*"},
    {name: "editormd/lib", dist: "./node_modules/editor.md-ext/lib/*.*"},
    {name: "editormd/plugins", dist: "./node_modules/editor.md-ext/plugins/*.*"},
    {name: "editormd/examples/js", dist: "./node_modules/editor.md-ext/examples/js/*.js"},
    {name: 'element-ui/lib', dist: './node_modules/element-ui/lib/index.js'},
    {name: 'element-ui/lib/theme-chalk', dist: './node_modules/element-ui/lib/theme-chalk/index.css'},
    {name: 'element-ui/lib/theme-chalk/fonts', dist: './node_modules/element-ui/lib/theme-chalk/fonts/*.*'},
    {name: 'font-awesome', dist: './node_modules/@fortawesome/fontawesome-free/**/*.*'},
    {name: 'highlight.js', dist: './node_modules/highlight.js/**/*.*'},
    // Post.BackendRender.cshtml tham chiếu ~/lib/highlight/highlight.min.js và ~/lib/highlight/styles/*.min.css
    {name: 'highlight', dist: './node_modules/@highlightjs/cdn-assets/highlight.min.js'},
    {name: 'highlight/styles', dist: './node_modules/@highlightjs/cdn-assets/styles/*.min.css'},
    {name: 'github-markdown-css', dist: './node_modules/github-markdown-css/*.css'},
    {name: 'imagesloaded', dist: './node_modules/imagesloaded/*.js'},
]

//Xóa các tệp đã nén
gulp.task("clean:css", done => rimraf(paths.minCssDist, done));
gulp.task("clean:js", done => rimraf(paths.minJsDist, done));

gulp.task("clean", gulp.series(["clean:js", "clean:css"]));

//Di chuyển các gói thành phần frontend tải bằng npm vào đường dẫn wwwroot
gulp.task("move:dist", done => {
    libs.forEach(item => {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name + "/dist"));
    });
    done()
})
gulp.task("move:custom", done => {
    customLibs.forEach(item => {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name))
    })
    done()
})

//Nén mỗi tệp css thành tệp tương ứng có đuôi min.css
gulp.task("min:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest('.'));
});

//Gộp, đóng gói và nén tất cả tệp css vào app.min.css
gulp.task("concat:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(concat(paths.concatCssDist))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

//Nén mỗi tệp js thành tệp tương ứng có đuôi min.js
gulp.task("min:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest('.'));
});

//Gộp, đóng gói và nén tất cả tệp js vào app.min.js
gulp.task("concat:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(concat(paths.concatJsDist))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});


gulp.task('move', gulp.series(['move:dist', 'move:custom']))
gulp.task("min", gulp.series(["min:js", "min:css"]))
gulp.task("concat", gulp.series(["concat:js", "concat:css"]))


//Tự động thực thi sau khi theo dõi thấy tệp thay đổi
gulp.task("auto", () => {
    gulp.watch(paths.css, gulp.series(["min:css", "concat:css"]));
    gulp.watch(paths.js, gulp.series(["min:js", "concat:js"]));
});