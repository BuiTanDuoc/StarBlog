<template>
  <div class="message-panel">
    <div class="message-header">Bạn có {{ data.length }} tin nhắn</div>
    <div class="message-content">
      <div v-for="item in data" :key="item.key" class="message-item">
        <div class="message-avatar">
          <img class="avatar" :src="require('@/assets/user.png')"/>
        </div>
        <span class="sender">
          {{ item.sender }}
        </span>
        <span class="time">
          <li class="fa fa-clock-o"></li> {{ item.time }}
        </span>
        <div class="message-cotent">
          {{ item.content }}
        </div>
      </div>
    </div>
    <div class="message-footer">Xem tất cả tin nhắn</div>
  </div>
</template>

<script>
export default {
  name: 'MessagePanel',
  props: {
    data: {
      type: Array,
      default: () => [
        {
          key: "1",
          avatar: '@/assets/user.png',
          content: 'Bạn đã đổi mật khẩu',
          sender: 'Người dùng 1',
          time: '5 phút trước'
        },
        {
          key: "2",
          avatar: '@/assets/user.png',
          content: 'Bạn đã thay đổi ảnh đại diện',
          sender: 'Người dùng 2',
          time: '2 giờ trước'
        },
        {
          key: "3",
          avatar: '@/assets/user.png',
          content: 'Hôm nay có 25 thành viên mới tham gia',
          sender: 'Người dùng 3',
          time: 'Hôm qua'
        },
        {
          key: "4",
          avatar: '@/assets/user.png',
          content: 'Bạn đã đăng một bài viết ngắn mới',
          sender: 'Người dùng 4',
          time: 'Hôm qua'
        },
        {
          key: "5",
          avatar: '@/assets/user.png',
          content: 'Bạn đã đăng một bài viết ngắn mới',
          sender: 'Người dùng 5',
          time: 'Hôm kia'
        }]
    }
  },
  data() {
    return {}
  },
  methods: {
    handleClick: function () {
      // Hàm xử lý thao tác nút
      this.$emit('click', {})
    }
  },
  mounted() {
  }
}
</script>

<style scoped>
.message-panel {
  font-size: 15px;
  width: 300px;
  margin: -12px;
}

.message-header {
  padding-left: 10px;
  font-size: 14px;
  padding-top: 6px;
  padding-bottom: 6px;
}

.message-content {
  font-size: 15px;
}

.message-item {
  border-color: rgba(180, 190, 190, 0.8);
  border-top-width: 1px;
  border-top-style: solid;
  padding-top: 10px;
  padding-bottom: 10px;
}

.message-item:hover {
  cursor: pointer;
  background: #b1a6a61e;
}

.message-avatar {
  padding-left: 10px;
}

.avatar {
  margin-right: 10px;
}

.sender {
  font-size: 16px;
  font-weight: bold;
}

.time {
  font-size: 12px;
  float: right;
  padding-right: 10px;
}

.message-footer {
  font-size: 14px;
  text-align: center;
  padding-top: 10px;
  padding-bottom: 10px;
  border-color: rgba(180, 190, 190, 0.8);
  border-top-width: 1px;
  border-top-style: solid;
}

.message-footer:hover {
  cursor: pointer;
  background: #b1a6a61e;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 90px;
  float: left;
}
</style>
