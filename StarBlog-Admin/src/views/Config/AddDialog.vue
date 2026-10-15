<template>
  <el-dialog title="Thêm cấu hình" :visible.sync="dialogFormVisible" width="30%">
    <el-form ref="uploadForm" :model="form" :rules="formRules" label-width="80px">
      <el-form-item label="key" prop="key">
        <el-input v-model="form.key" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="value" prop="value">
        <el-input v-model="form.value" autocomplete="off"></el-input>
      </el-form-item>
      <el-form-item label="description" prop="description">
        <el-input v-model="form.description" autocomplete="off"></el-input>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button @click="close">Hủy</el-button>
      <el-button type="primary" @click="submitUpload">Xác nhận</el-button>
    </div>
  </el-dialog>
</template>

<script>
export default {
  name: "AddConfigDialog",
  props: {
    onAddSucceed: {
      type: Function
    },
    onUpdateSucceed: {
      type: Function
    }
  },
  data() {
    return {
      dialogFormVisible: false,
      mode: 'add',
      form: {
        key: '',
        value: '',
        description: null
      },
      formRules: {
        key: [{required: true, message: 'Vui lòng nhập key', trigger: 'blur'},],
        value: [{required: true, message: 'Vui lòng nhập value', trigger: 'blur'}],
      }
    }
  },
  methods: {
    resetForm() {
      this.$refs.uploadForm.resetFields()
      this.form = {
        key: '',
        value: '',
        description: null
      }
    },
    show() {
      this.dialogFormVisible = true
    },
    hide() {
      this.dialogFormVisible = false
    },
    close() {
      this.resetForm()
      this.hide()
    },
    edit(item) {
      this.mode = 'edit'
      this.form = item
      this.show()
    },
    submitUpload() {
      this.$refs.uploadForm.validate((valid) => {
        if (!valid) return false

        if (this.mode === 'add') {
          this.$api.config.add(this.form)
            .then(res => {
              if (res.successful) {
                this.$message({message: 'Thêm cấu hình thành công', type: 'success'})
                this.$emit('onAddSucceed')
                this.close()
              }
            })
            .catch(res => {
              this.$message({message: `Thêm cấu hình thất bại! ${res.message}`, type: 'error'})
            })
        }

        if (this.mode === 'edit') {
          this.$api.config.update(this.form)
            .then(res => {
              if (res.successful) {
                this.$message({message: 'Sửa cấu hình thành công', type: 'success'})
                this.$emit('onUpdateSucceed')
                this.close()
              }
            })
            .catch(res => {
              this.$message({message: `Sửa cấu hình thất bại! ${res.message}`, type: 'error'})
            })
        }
      })
    },
  }
}
</script>

<style scoped>

</style>
