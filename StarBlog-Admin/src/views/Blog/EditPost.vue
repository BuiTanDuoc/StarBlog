<template>
  <div class="pb-5">
    <el-breadcrumb separator-class="el-icon-arrow-right" class="my-3">
      <el-breadcrumb-item :to="{ path: '/' }">Trang chủ</el-breadcrumb-item>
      <el-breadcrumb-item>Blog</el-breadcrumb-item>
      <el-breadcrumb-item :to="{ path: '/post/list' }">Quản lý bài viết</el-breadcrumb-item>
      <el-breadcrumb-item v-if="mode==='edit'">Sửa bài viết</el-breadcrumb-item>
      <el-breadcrumb-item v-if="mode==='new'">Tạo bài viết mới</el-breadcrumb-item>
    </el-breadcrumb>

    <el-divider></el-divider>

    <div class="d-flex justify-content-between mb-1">
      <div>Tiêu đề</div>
      <el-link v-if="post!==null" type="primary" target="_blank">{{ `${baseUrl}/Blog/Post/${post.id}` }}</el-link>
    </div>
    <el-input v-model="postTitle" placeholder="Tiêu đề bài viết" class="mb-3"></el-input>

    <div class="mb-1">Nội dung</div>
    <v-md-editor
      class="mb-4"
      v-model="postContent"
      :default-show-toc="true"
      :codemirror-style-reset="true"
      :disabled-menus="mode==='edit' ? [] : ['image/upload-image']"
      @save="onEditorSave"
      @fullscreen-change="fullscreenChange"
      @upload-image="handleUploadImage"
      height="750px"/>

    <el-divider></el-divider>

    <el-descriptions title="Thông tin bài viết" :column="2" border>
      <el-descriptions-item label="ID bài viết">{{ post.id }}</el-descriptions-item>
      <el-descriptions-item label="Trạng thái">
        <el-tag size="small">{{ post.status }}</el-tag>
      </el-descriptions-item>
      <el-descriptions-item label="Thời gian thêm">{{ post.creationTime }}</el-descriptions-item>
      <el-descriptions-item label="Thời gian sửa">{{ post.lastUpdateTime }}</el-descriptions-item>
    </el-descriptions>

    <el-divider content-position="left">Tùy chọn bài viết</el-divider>

    <el-form label-position="left" label-width="auto" :model="form" class="text-start">
      <el-form-item label="Chế độ" prop="isPublish">
        <el-radio v-model="form.isPublish" :label="true">Đăng</el-radio>
        <el-radio v-model="form.isPublish" :label="false">Lưu bản nháp</el-radio>
      </el-form-item>

      <el-row :gutter="20">
        <el-col :span="8">
          <el-form-item label="Slug" prop="slug">
            <el-input v-model="form.slug" maxlength="150" show-word-limit placeholder="Slug"></el-input>
            <small>Tên địa chỉ thân thiện, chỉ được dùng chữ cái, số, dấu gạch nối -, dấu gạch dưới _, không quá 150 ký tự</small>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="Danh mục" prop="category">
            <el-cascader class="w-100" :options="categoryTree" clearable :props="{
                checkStrictly:true,
                expandTrigger:'hover',
                emitPath:false,
              }" v-model="form.category" filterable></el-cascader>
          </el-form-item>
        </el-col>
        <el-col :span="8">
          <el-form-item label="Đánh dấu bài viết" prop="status">
            <el-input v-model="form.status" maxlength="20" show-word-limit placeholder="Status"></el-input>
            <small>Trích xuất tiền tố tên tệp của tệp markdown gốc, dùng để phân biệt trạng thái bài viết. Ví dụ: chưa hoàn thành, bài cũ</small>
          </el-form-item>
        </el-col>
      </el-row>

      <el-form-item label="Giới thiệu" prop="summary">
        <el-input type="textarea" v-model="form.summary" :rows="8" maxlength="200" show-word-limit
                  placeholder="Giới thiệu"></el-input>
      </el-form-item>
    </el-form>

    <div class="text-end">
      <el-button type="primary" plain @click="onSaveClick" :loading="saveButtonLoading">Lưu</el-button>
    </div>
  </div>
</template>

<script>
// todo Nên thêm cảnh báo khi đóng trang, hoặc chức năng tự động lưu khi đóng trang~
import {baseUrl} from "@/utils/global";
import {dateTimeBeautify} from "@/utils/dateTime";
import VMdEditor from "@kangc/v-md-editor/lib/codemirror-editor";

export default {
  name: "EditPost",
  components: {
    VMdEditor
  },
  data() {
    return {
      baseUrl: baseUrl,
      // Chế độ chỉnh sửa: new / edit
      mode: 'new',
      postTitle: '',
      postContent: '',
      post: null,
      categories: [],
      categoryTree: [],
      form: {
        isPublish: false,
        status: '',
        slug: '',
        summary: '',
        category: 0,
      },
      saveButtonLoading: false,
    }
  },
  mounted() {
    this.init()
    this.loadCategories()
  },
  methods: {
    // Chuyển đổi toàn màn hình
    fullscreenChange(isFullscreen) {
      this.$store.commit('onFullscreenChange')
    },
    /**
     * Xử lý tải ảnh lên
     * @param event
     * @param insertImage Function
     * @param files
     */
    handleUploadImage(event, insertImage, files) {
      let file = files[0]

      this.$api.blogPost.uploadImage(this.post.id, file)
        .then(res => {
          this.$message.success(`Thêm ảnh: ${res.data.imgName}`)
          insertImage({
            url: res.data.imgUrl.replaceAll('\\', '/'),
            desc: res.data.imgName
          })
        })
        .catch(res => this.$message.error(`Tải ảnh lên thất bại. ${res.message}`))
    },
    // Khởi tạo
    init() {
      let id = this.$route.params.id
      if (id) {
        this.mode = 'edit'
        this.$api.blogPost.get(id)
          .then(res => {
            this.post = res.data
            this.postTitle = this.post.title
            this.postContent = this.post.content
            this.post.creationTime = dateTimeBeautify(this.post.creationTime)
            this.post.lastUpdateTime = dateTimeBeautify(this.post.lastUpdateTime)
            this.form.slug = this.post.slug
            this.form.status = this.post.status
            this.form.isPublish = this.post.isPublish
            this.form.summary = this.post.summary
            this.form.category = this.post.categoryId
            this.$notify.info({title: 'Chế độ hiện tại: sửa bài viết', message: `Tải bài viết: ${this.postTitle}`})
          })
          .catch(res => this.$message.error(`Lấy thông tin thất bại. ${res.message}`))
      } else {
        this.mode = 'new'
        this.post = {}
        this.$notify.warning({title: 'Chế độ hiện tại: tạo bài viết mới', message: 'Lưu ý: chỉ có thể tải ảnh lên sau khi đã lưu bài viết!'})
      }
    },
    loadCategories() {
      const mapNodes = (nodes) => {
        let items = []
        if (!nodes) return null
        for (const node of nodes) {
          items.push({
            label: `${node.text} (${node.tags[0]})`,
            value: node.id,
            children: mapNodes(node.nodes)
          })
        }
        return items
      }

      this.$api.category.getNodes().then(res => this.categoryTree = mapNodes(res.data))
      this.$api.category.getAll().then(res => this.categories = res.data)
    },
    onEditorSave(text, html) {
      this.save()
    },
    onSaveClick() {
      this.save()
    },
    save() {
      let post = this.post
      post.title = this.postTitle
      post.content = this.postContent
      post.isPublish = this.form.isPublish
      post.slug = this.form.slug
      post.status = this.form.status
      post.summary = this.form.summary
      post.categoryId = this.form.category

      this.saveButtonLoading = true

      if (this.mode === 'new') {
        this.$api.blogPost.add(post)
          .then(res => {
            this.$message.success(`Lưu thành công.`)
            // Tạo xong thì chuyển sang trang chỉnh sửa
            this.$router.push(`/post/edit/${res.data.id}`)
          })
          .catch(res => {
            console.log(res)
            this.$message.error(`Thao tác thất bại. ${res.message}`)
          })
          .finally(() => this.saveButtonLoading = false)
      } else {
        this.$api.blogPost.update(post)
          .then(res => this.$message.success(`Lưu thành công.`))
          .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          .finally(() => this.saveButtonLoading = false)
      }
    }
  }
}
</script>

<style lang="scss">
.v-md-editor {
  text-align: start;

  .codemirror-wrapper {
    .CodeMirror {
      font-size: 18px !important;
    }
  }
}
</style>
