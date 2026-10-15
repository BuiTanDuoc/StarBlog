<template>
  <el-dialog title="Sửa ảnh" :visible.sync="dialogVisible" width="30%">
    <el-form ref="editForm" :model="form" :rules="formRules" label-width="80px">
      <el-form-item label="Tên ảnh" prop="title">
        <el-input v-model="form.title" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="Địa điểm chụp" prop="location">
        <el-input v-model="form.location" autocomplete="off"></el-input>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button @click="close">Hủy</el-button>
      <el-button type="primary" @click="submitUpdate">Lưu</el-button>
    </div>
  </el-dialog>
</template>

<script>
export default {
  name: 'EditPhotoDialog',
  data() {
    return {
      dialogVisible: false,
      form: {
        id: null,
        title: '',
        location: '',
      },
      formRules: {
        title: [
          { required: true, message: 'Vui lòng nhập tiêu đề ảnh', trigger: 'blur' },
          { min: 1, max: 200, message: 'Độ dài từ 1 đến 200 ký tự', trigger: 'blur' }
        ],
        location: [{ required: true, message: 'Vui lòng nhập địa điểm chụp', trigger: 'blur' }],
      }
    }
  },
  methods: {
    show(photo) {
      this.form.id = photo.id
      this.form.title = photo.title || ''
      this.form.location = photo.location || ''
      this.dialogVisible = true
    },
    hide() {
      this.dialogVisible = false
    },
    close() {
      this.hide()
      this.$refs.editForm && this.$refs.editForm.resetFields()
    },
    submitUpdate() {
      this.$refs.editForm.validate(async (valid) => {
        if (!valid) return
        try {
          await this.$api.photo.update({ id: this.form.id, title: this.form.title, location: this.form.location })
          this.$message.success('Cập nhật thông tin ảnh thành công')
          this.$emit('onUpdateSucceed')
          this.close()
        } catch (err) {
          this.$message.error(`Cập nhật thất bại: ${err.message || err}`)
        }
      })
    }
  }
}
</script>

<style scoped>
</style>