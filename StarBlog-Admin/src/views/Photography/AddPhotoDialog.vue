<template>
  <el-dialog title="Tải ảnh lên" :visible.sync="dialogFormVisible" width="30%">
    <el-form ref="uploadForm" :model="form" :rules="formRules" label-width="80px">
      <el-form-item label="Tên ảnh" prop="title">
        <el-input v-model="form.title" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="Địa điểm chụp" prop="location">
        <el-input v-model="form.location" autocomplete="off"></el-input>
      </el-form-item>
    </el-form>
    <el-upload ref="upload" drag action=""
               accept="image/jpeg,image/png"
               :file-list="fileList"
               :on-change="onUploadChange"
               :auto-upload="false">
      <i class="el-icon-upload"></i>
      <div class="el-upload__text">Kéo tệp vào đây, hoặc <em>nhấp để tải lên</em></div>
      <div class="el-upload__tip" slot="tip">Chỉ được tải lên tệp jpg/png, và không vượt quá 500kb</div>
    </el-upload>
    <div slot="footer" class="dialog-footer">
      <el-button @click="close">Hủy</el-button>
      <el-button type="primary" @click="submitUpload">Xác nhận</el-button>
    </div>
  </el-dialog>
</template>

<script>
export default {
  name: "AddPhotoDialog",
  props: {
    onAddPhotoSucceed: {
      type: Function
    }
  },
  data() {
    return {
      dialogFormVisible: false,
      fileList: [],
      form: {
        title: '',
        location: '',
        file: null
      },
      formRules: {
        title: [
          {required: true, message: 'Vui lòng nhập tiêu đề ảnh', trigger: 'blur'},
          {min: 1, max: 200, message: 'Độ dài từ 1 đến 200 ký tự', trigger: 'blur'}
        ],
        location: [{required: true, message: 'Vui lòng nhập địa điểm chụp', trigger: 'blur'}],
      }
    }
  },
  methods: {
    onUploadChange(file, fileList) {
      const isIMAGE = (file.raw.type === 'image/jpeg' || file.raw.type === 'image/png')
      if (!isIMAGE) {
        this.$message.error('Chỉ được tải lên ảnh jpg/png!')
        return false
      }

      const isLt500KB = file.raw.size / 1024 <= 500
      if (!isLt500KB) {
        this.$message.error('Kích thước ảnh không được vượt quá 500KB')
        return false
      }

      if (fileList.length > 0) {
        this.fileList = [fileList[fileList.length - 1]]
      }

      this.form.file = file
    },
    show() {
      this.dialogFormVisible = true
    },
    hide() {
      this.dialogFormVisible = false
    },
    close() {
      this.hide()
      this.$refs.uploadForm.resetFields()
      this.fileList = []
    },
    submitUpload() {
      this.$refs.uploadForm.validate((valid) => {
        if (!valid) return false

        this.$api.photo.add(this.form.title, this.form.location, this.form.file.raw)
          .then(res => {
            if (res.successful) {
              this.$message({message: 'Thêm ảnh thành công', type: 'success'})
              this.$emit('onAddPhotoSucceed')
              this.close()
            }
          })
      })
    },
  }
}
</script>

<style scoped>

</style>
