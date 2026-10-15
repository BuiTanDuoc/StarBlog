<template>
  <div class="login-page">
    <!-- Trang trí nền -->
    <div class="background-decoration">
      <div class="circle circle-1"></div>
      <div class="circle circle-2"></div>
      <div class="circle circle-3"></div>
    </div>

    <!-- Thẻ đăng nhập -->
    <div class="login-card">
      <!-- Logo và tiêu đề phần đầu -->
      <div class="login-header">
        <div class="logo">
          <i class="el-icon-star-on"></i>
        </div>
        <h1 class="title">StarBlog</h1>
        <p class="subtitle">Đăng nhập trang quản trị</p>
      </div>

      <!-- Biểu mẫu đăng nhập -->
      <el-form :model="loginForm" :rules="fieldRules" ref="loginForm" class="login-form" @keyup.enter.native="login">
        <el-form-item prop="username">
          <div class="input-wrapper">
            <i class="el-icon-user input-icon"></i>
            <el-input type="text" v-model="loginForm.username" auto-complete="off" placeholder="Vui lòng nhập tên người dùng"
              class="custom-input"></el-input>
          </div>
        </el-form-item>

        <el-form-item prop="password">
          <div class="input-wrapper">
            <el-input type="password" v-model="loginForm.password" auto-complete="off" placeholder="Vui lòng nhập mật khẩu"
              class="custom-input" show-password></el-input>
          </div>
        </el-form-item>

        <el-form-item class="button-group">
          <el-button type="primary" class="login-btn" @click="login" :loading="loading" size="large">
            <span v-if="!loading">Đăng nhập</span>
            <span v-else>Đang đăng nhập...</span>
          </el-button>

          <el-button type="text" class="reset-btn" @click="reset">
            Đặt lại biểu mẫu
          </el-button>
        </el-form-item>
      </el-form>

      <!-- Thông tin phần chân -->
      <div class="login-footer">
        <p>© 2024 StarBlog. All rights reserved.</p>
      </div>
    </div>
  </div>
</template>

<script>
import * as auth from '@/utils/auth'

export default {
  name: "Login",
  data() {
    return {
      loading: false,
      loginForm: {
        username: '',
        password: '',
      },
      fieldRules: {
        username: [{ required: true, message: 'Vui lòng nhập tên người dùng', trigger: 'blur' }],
        password: [{ required: true, message: 'Vui lòng nhập mật khẩu', trigger: 'blur' }]
      },
    }
  },
  methods: {
    login() {
      this.loading = true
      this.$api.auth.login(this.loginForm)
        .then(res => {  // Gọi API đăng nhập
          // Lưu thông tin đăng nhập
          auth.login(res.data.token, this.loginForm.username, res.data.expiration)

          // Đăng nhập thành công, chuyển đến trang chủ
          this.$message.success('Đăng nhập thành công')
          this.$router.push('/')
        })
        .catch(err => this.$message.error(`Đăng nhập thất bại: ${err.message}`))
        .finally(() => this.loading = false)
    },
    reset() {
      this.$refs.loginForm.resetFields()
    }
  },
}
</script>

<style lang="scss" scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
  overflow: hidden;

  // Vòng tròn trang trí nền
  .background-decoration {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;

    .circle {
      position: absolute;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.1);
      animation: float 6s ease-in-out infinite;

      &.circle-1 {
        width: 200px;
        height: 200px;
        top: 10%;
        left: 10%;
        animation-delay: 0s;
      }

      &.circle-2 {
        width: 150px;
        height: 150px;
        top: 60%;
        right: 15%;
        animation-delay: 2s;
      }

      &.circle-3 {
        width: 100px;
        height: 100px;
        bottom: 20%;
        left: 20%;
        animation-delay: 4s;
      }
    }
  }
}

.login-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  padding: 40px;
  width: 100%;
  max-width: 420px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  animation: slideUp 0.8s ease-out;

  .login-header {
    text-align: center;
    margin-bottom: 40px;

    .logo {
      width: 80px;
      height: 80px;
      margin: 0 auto 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);

      i {
        font-size: 36px;
        color: white;
      }
    }

    .title {
      font-size: 32px;
      font-weight: 700;
      color: #2c3e50;
      margin: 0 0 8px 0;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .subtitle {
      font-size: 16px;
      color: #7f8c8d;
      margin: 0;
      font-weight: 400;
    }
  }

  .login-form {
    .el-form-item {
      margin-bottom: 24px;

      .input-wrapper {
        position: relative;

        .input-icon {
          position: absolute;
          right: 16px;
          top: 50%;
          transform: translateY(-50%);
          color: #bdc3c7;
          font-size: 18px;
          z-index: 10;
          pointer-events: none;
        }

        .custom-input {
           >>> .el-input__inner {
             height: 50px;
             padding-left: 20px;
             padding-right: 50px;
             border: 2px solid #ecf0f1;
             border-radius: 12px;
             font-size: 16px;
             background: #f8f9fa;
             transition: all 0.3s ease;
             position: relative;
             z-index: 1;

             &:focus {
               border-color: #667eea;
               background: white;
               box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
             }
           }

           >>> .el-input__inner::placeholder {
             color: #bdc3c7;
             opacity: 1;
           }
         }
      }

      &.button-group {
        margin-top: 32px;
        margin-bottom: 0;

        .login-btn {
          width: 100%;
          height: 50px;
          border-radius: 12px;
          font-size: 16px;
          font-weight: 600;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          border: none;
          margin-bottom: 16px;
          transition: all 0.3s ease;

          &:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
          }

          &:active {
            transform: translateY(0);
          }
        }

        .reset-btn {
          color: #7f8c8d;
          font-size: 14px;

          &:hover {
            color: #667eea;
          }
        }
      }
    }
  }

  .login-footer {
    text-align: center;
    margin-top: 32px;
    padding-top: 24px;
    border-top: 1px solid #ecf0f1;

    p {
      color: #95a5a6;
      font-size: 14px;
      margin: 0;
    }
  }
}

// Hiệu ứng động
@keyframes float {

  0%,
  100% {
    transform: translateY(0px);
  }

  50% {
    transform: translateY(-20px);
  }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// Thiết kế responsive
@media (max-width: 768px) {
  .login-card {
    margin: 20px;
    padding: 30px 24px;
    max-width: none;

    .login-header {
      margin-bottom: 30px;

      .logo {
        width: 60px;
        height: 60px;

        i {
          font-size: 28px;
        }
      }

      .title {
        font-size: 28px;
      }

      .subtitle {
        font-size: 14px;
      }
    }
  }

  .background-decoration {
    .circle {
      &.circle-1 {
        width: 120px;
        height: 120px;
      }

      &.circle-2 {
        width: 80px;
        height: 80px;
      }

      &.circle-3 {
        width: 60px;
        height: 60px;
      }
    }
  }
}

@media (max-width: 480px) {
  .login-card {
    margin: 16px;
    padding: 24px 20px;

    .login-form {
      .el-form-item {
        .input-wrapper {
          .custom-input {
             >>> .el-input__inner {
               height: 46px;
               font-size: 15px;
             }
           }
        }

        &.button-group {
          .login-btn {
            height: 46px;
            font-size: 15px;
          }
        }
      }
    }
  }
}
</style>
