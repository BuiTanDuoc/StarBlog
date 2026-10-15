const initTreeView = function (data) {
    return $('#post-toc-container').treeview({
        data,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
    })
}

$(function () {
    let markdownContent = document.getElementById('post-markdown-content')
    if (markdownContent) {
        let editorMdView = editormd.markdownToHTML("post-markdown-content", {
            htmlDecode: true,
            tocm: true,    // Using [TOCM]
            emoji: true,
            taskList: true,
            tex: true,  // Mặc định không phân tích
            flowChart: true,  // Mặc định không phân tích
            sequenceDiagram: true,  // Mặc định không phân tích
        });

        initTreeView(editorMdView.markdownTocTree)
    }
})

/**
 * Chuyển đổi liên kết hình ảnh trong bài viết
 *
 * @deprecated Giờ không cần nữa, xử lý trực tiếp ở backend
 * @param postId
 */
function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log('procImages', res)
        for (const imgElem of document.querySelectorAll('.post-content img')) {
            let originSrc = imgElem.getAttribute('src')
            // let newSrc = `/assets/blog/${res.data.path}/${originSrc}`
            let newSrc = `/media/blog/${res.data.id}/${originSrc}`
            console.log('originSrc', originSrc)
            console.log('newSrc', newSrc)
            imgElem.setAttribute('src', newSrc)
        }
    })
}