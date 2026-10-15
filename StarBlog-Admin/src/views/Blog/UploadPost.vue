<template>
  <el-row>
    <el-col :span="8" :offset="8">
      <el-form ref="uploadForm" :model="form" :rules="formRules" label-width="auto" label-position="top">
        <el-form-item label="Tiêu đề bài viết" prop="title">
          <el-input v-model="form.title" autocomplete="off"></el-input>
        </el-form-item>
        <el-form-item label="Mô tả bài viết" prop="summary">
          <el-input v-model="form.summary" autocomplete="off"></el-input>
        </el-form-item>
        <el-form-item label="Danh mục bài viết" prop="categoryId">
          <el-cascader class="w-100" :options="categoriesTree" v-model="form.categoryId" filterable clearable
                       placeholder="Lọc danh mục"
                       :props="{
                              checkStrictly:true,
                              expandTrigger: 'hover',
                              emitPath:false,
                         }"></el-cascader>
        </el-form-item>
        <el-form-item label="Mã hóa tệp nén ZIP" prop="zipEncoding">
          <el-select v-model="form.zipEncoding" class="w-100" clearable filterable placeholder="Mã hóa tệp nén ZIP">
            <el-option v-for="item in zipCodings" :key="item" :label="item" :value="item"/>
          </el-select>
        </el-form-item>
      </el-form>
      <el-upload
        class="w-100"
        ref="upload" drag action=""
        accept="application/x-zip-compressed,.zip"
        :file-list="fileList"
        :on-change="onUploadChange"
        :auto-upload="false"
      >
        <i class="el-icon-upload"></i>
        <div class="el-upload__text">Kéo tệp vào đây, hoặc <em>nhấp để tải lên</em></div>
        <div class="el-upload__tip" slot="tip">Chỉ được tải lên tệp zip</div>
      </el-upload>

      <el-row type="flex" justify="end" class="py-3">
        <el-button type="primary" @click="submitUpload" :loading="loading">Xác nhận</el-button>
      </el-row>
    </el-col>
  </el-row>
</template>

<script>
export default {
  name: "UploadPost",
  data() {
    return {
      loading: false,
      dialogFormVisible: false,
      fileList: [],
      categoriesTree: [],
      zipCodings: ['utf-8', 'utf-16', 'gbk', 'gb2312'],
      form: {
        title: '',
        summary: '',
        categoryId: null,
        zipEncoding: 'utf-8',
        file: null
      },
      formRules: {
        title: [
          {required: true, message: 'Vui lòng nhập tiêu đề bài viết', trigger: 'blur'},
          {min: 1, max: 200, message: 'Độ dài từ 1 đến 200 ký tự', trigger: 'blur'}
        ],
        summary: [{required: true, message: 'Vui lòng nhập giới thiệu bài viết', trigger: 'blur'}],
        categoryId: [{required: true, message: 'Vui lòng chọn danh mục bài viết', trigger: 'blur'}],
      }
    }
  },
  mounted() {
    this.loadCategories()
  },
  methods: {
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

      this.$api.category.getNodes()
        .then(res => this.categoriesTree = mapNodes(res.data))
        .catch(res => this.$message.error(`Lỗi khi tải danh sách danh mục: ${res.message}`))
    },
    onUploadChange(file, fileList) {
      console.log(file.raw.type)
      const isIMAGE = (file.raw.type === 'application/x-zip-compressed')
      if (!isIMAGE) {
        this.$message.error('Chỉ được tải lên tệp nén zip!')
        return false
      }

      if (fileList.length > 0) {
        this.fileList = [fileList[fileList.length - 1]]
      }

      this.form.file = file
    },
    submitUpload() {
      this.$refs.uploadForm.validate((valid) => {
        if (!valid) return false

        this.loading = true

        this.$api.blog.upload(this.form.title, this.form.summary, this.form.categoryId, this.form.file.raw, this.form.zipEncoding)
          .then(res => {
            if (res.successful) {
              this.$message({message: 'Tải bài viết lên thành công', type: 'success'})
              this.$router.push('/post/list')
            }
          })
          .catch(res => {
            this.$alert(res.message, 'Tải bài viết lên thất bại', {
              confirmButtonText: 'Xác nhận',
              type: 'error'
            })
          })
          .finally(() => this.loading = false)
      })
    },
  }
}
</script>

<style lang="scss">
.el-upload {
  width: 100% !important;
}

.el-upload-dragger {
  width: 100% !important;
}

</style>
