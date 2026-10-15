<template>
  <el-dialog title="Thông tin danh mục đề xuất" :visible.sync="dialogFormVisible">
    <el-form ref="form" :model="form">
      <el-form-item label="Tên đề xuất" :label-width="formLabelWidth">
        <el-input v-model="form.name" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="Mô tả" :label-width="formLabelWidth">
        <el-input v-model="form.description" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="Tên class biểu tượng" :label-width="formLabelWidth">
        <el-input v-model="form.iconCssClass" autocomplete="off"></el-input>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button @click="close">Hủy</el-button>
      <el-button type="primary" @click="submit">Xác nhận</el-button>
    </div>
  </el-dialog>
</template>

<script>
export default {
  name: "SetFeaturedDialog",
  props: {
    category: {
      type: Object
    }
  },
  data() {
    return {
      dialogFormVisible: false,
      formLabelWidth: '120px',
      form: {
        name: '',
        description: '',
        iconCssClass: ''
      },
      formRules: {
        name: [
          {required: true, message: 'Vui lòng nhập tên đề xuất', trigger: 'blur'},
          {min: 1, max: 20, message: 'Độ dài từ 1 đến 20 ký tự', trigger: 'blur'}
        ],
        description: [
          {required: true, message: 'Vui lòng nhập mô tả', trigger: 'blur'},
          {min: 1, max: 300, message: 'Độ dài từ 1 đến 300 ký tự', trigger: 'blur'}
        ],
      }
    }
  },
  methods: {
    show() {
      console.log(this.category)
      this.dialogFormVisible = true
    },
    hide() {
      this.dialogFormVisible = false
    },
    close() {
      this.hide()
      this.$refs.form.resetFields()
    },
    submit() {
      this.$refs.form.validate((valid) => {
        if (!valid) return false

        this.$api.category.setFeatured(this.category.id, this.form)
          .then(res => {
            this.$message.success(`Thao tác thành công. ${res.message}`)
            this.$emit('onAddPhotoSucceed')
          })
          .catch(res => this.$message.error(`Thao tác thất bại. ${res.message}`))
          .finally(() => this.close())
      })
    },
  }
}
</script>

<style scoped>

</style>
