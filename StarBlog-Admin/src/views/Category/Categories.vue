<template>
  <div class="categories-container">
    <!-- Thanh thao tác phần đầu -->
    <div class="header-actions">
      <div class="search-section">
        <el-input
          v-model="search"
          placeholder="Tìm danh mục..."
          prefix-icon="el-icon-search"
          size="medium"
          style="width: 300px;"
          clearable
        />
      </div>
      <div class="action-buttons">
        <el-button
          type="primary"
          icon="el-icon-plus"
          @click="handleAdd"
          size="medium"
        >
          Thêm danh mục
        </el-button>
      </div>
    </div>

    <!-- Lưới thẻ danh mục -->
    <div class="categories-grid" v-loading="loading">
      <div
        v-for="category in filteredCategories"
        :key="category.id"
        class="category-card"
        @click="viewCategoryDetail(category)"
      >
        <div class="card-header">
          <div class="category-icon">
            <i class="el-icon-folder-opened"></i>
          </div>
          <div class="category-info">
            <h3 class="category-name">{{ category.name }}</h3>
            <p class="category-description">{{ category.description || 'Chưa có mô tả' }}</p>
          </div>
          <div class="category-status">
            <el-tag
              :type="category.visible ? 'success' : 'info'"
              size="mini"
            >
              {{ category.visible ? 'Hiển thị' : 'Ẩn' }}
            </el-tag>
          </div>
        </div>

        <div class="card-content">
          <div class="stats">
            <div class="stat-item">
              <span class="stat-label">Số lượng bài viết</span>
              <span class="stat-value">{{ category.posts || 0 }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">Danh mục cha</span>
              <span class="stat-value">{{ category.parentId || 'Không' }}</span>
            </div>
          </div>
        </div>

        <div class="card-actions" @click.stop>
          <el-button
            size="mini"
            type="text"
            @click="viewCategoryDetail(category)"
            icon="el-icon-document"
          >
            Bài viết
          </el-button>
          <el-button
            size="mini"
            type="text"
            @click="toggleVisibility(category)"
            :icon="category.visible ? 'el-icon-view' : 'el-icon-hide'"
          >
            {{ category.visible ? 'Ẩn' : 'Hiển thị' }}
          </el-button>
          <el-button
            size="mini"
            type="text"
            @click="setFeatured(category)"
            icon="el-icon-star-off"
          >
            Đề xuất
          </el-button>
          <el-button
            size="mini"
            type="text"
            @click="handleDelete(category)"
            icon="el-icon-delete"
            class="delete-btn"
          >
            Xóa
          </el-button>
        </div>
      </div>

      <!-- Trạng thái trống -->
      <div v-if="filteredCategories.length === 0 && !loading" class="empty-state">
        <i class="el-icon-folder-opened empty-icon"></i>
        <p class="empty-text">Chưa có dữ liệu danh mục</p>
        <el-button type="primary" @click="handleAdd">Tạo danh mục đầu tiên</el-button>
      </div>
    </div>

    <!-- Thành phần hộp thoại -->
    <add-category-dialog
      ref="addDialog"
      @onAddSucceed="onAddSucceed"
      @onUpdateSucceed="onUpdateSucceed"
    />
    <set-featured-dialog
      v-for="(category, index) in data"
      :key="`featured-${category.id}`"
      :ref="`setFeaturedDialog_${index}`"
      :category="category"
    />
  </div>
</template>

<script>
import AddCategoryDialog from "@/views/Category/AddDialog.vue";
import SetFeaturedDialog from "./SetFeaturedDialog";

export default {
  name: "Categories",
  components: {
    AddCategoryDialog,
    SetFeaturedDialog
  },
  data() {
    return {
      loading: false,
      saving: false,
      data: [],
      search: ''
    }
  },
  computed: {
    filteredCategories() {
      return this.data.filter(item =>
        !this.search || item.name.toLowerCase().includes(this.search.toLowerCase())
      )
    }
  },
  mounted() {
    this.loadData()
  },
  methods: {
    // Tải dữ liệu
    async loadData() {
      this.loading = true
      try {
        const res = await this.$api.category.getAll()
        this.data = res.data
      } catch (error) {
        console.error(error)
        this.$message.error(error.message)
      } finally {
        this.loading = false
      }
    },

    // Chuyển đến trang chi tiết danh mục
    viewCategoryDetail(category) {
      this.$router.push({
        name: 'Chi tiết danh mục',
        params: { id: category.id }
      })
    },

    // Chức năng gốc
    handleAdd() {
      this.$refs.addDialog.add()
    },

    handleEdit(item) {
      this.$refs.addDialog.edit(item)
    },

    handleDelete(item) {
      this.$confirm('Thao tác này sẽ xóa danh mục này, bạn có muốn tiếp tục không?', 'Thông báo', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(async () => {
        try {
          const res = await this.$api.category.deleteItem(item.id)
          this.$message.success(`Xóa thành công. ${res.message}`)
          this.loadData()
        } catch (error) {
          this.$message.error(`Thao tác thất bại. ${error.message}`)
        }
      }).catch(() => {
        this.$message('Đã hủy xóa')
      })
    },

    async toggleVisibility(item) {
      try {
        if (item.visible) {
          await this.$api.category.setInvisible(item.id)
        } else {
          await this.$api.category.setVisible(item.id)
        }
        this.$message.success('Thao tác thành công')
        this.loadData()
      } catch (error) {
        this.$message.error(`Thao tác thất bại: ${error.message}`)
      }
    },

    setFeatured(item) {
      const index = this.data.findIndex(cat => cat.id === item.id)
      if (index > -1) {
        this.$refs[`setFeaturedDialog_${index}`][0].show()
      }
    },

    onAddSucceed() {
      this.$message.success('Thêm thành công')
      this.loadData()
    },

    onUpdateSucceed() {
      this.$message.success('Lưu thành công')
      this.loadData()
    },

    // Phương thức tiện ích
    formatDate(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      return date.toLocaleDateString('vi-VN')
    }
  }
}
</script>

<style scoped>
.categories-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: 100vh;
}

/* Thanh thao tác phần đầu */
.header-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 16px 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.action-buttons {
  display: flex;
  gap: 12px;
}

/* Lưới thẻ danh mục */
.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.category-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  cursor: pointer;
  border: 2px solid transparent;
}

.category-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: #409eff;
}

.card-header {
  display: flex;
  align-items: flex-start;
  margin-bottom: 16px;
}

.category-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #409eff, #67c23a);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16px;
  flex-shrink: 0;
}

.category-icon i {
  font-size: 24px;
  color: white;
}

.category-info {
  flex: 1;
  min-width: 0;
}

.category-name {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.category-description {
  font-size: 14px;
  color: #909399;
  margin: 0;
  line-height: 1.4;
}

.category-status {
  margin-left: 12px;
}

.card-content {
  margin-bottom: 16px;
}

.stats {
  display: flex;
  gap: 24px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-label {
  font-size: 12px;
  color: #909399;
  margin-bottom: 4px;
}

.stat-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.card-actions {
  display: flex;
  gap: 8px;
  padding-top: 16px;
  border-top: 1px solid #f0f0f0;
}

.card-actions .el-button {
  padding: 4px 8px;
  font-size: 12px;
}

.delete-btn {
  color: #f56c6c !important;
}

.delete-btn:hover {
  background-color: #fef0f0 !important;
}

/* Trạng thái trống */
.empty-state {
  grid-column: 1 / -1;
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.empty-icon {
  font-size: 64px;
  color: #c0c4cc;
  margin-bottom: 16px;
}

.empty-text {
  font-size: 16px;
  color: #909399;
  margin: 0 0 20px 0;
}

/* Thiết kế responsive */
@media (max-width: 768px) {
  .categories-grid {
    grid-template-columns: 1fr;
  }

  .header-actions {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }
}

@media (max-width: 480px) {
  .categories-container {
    padding: 12px;
  }

  .category-card {
    padding: 16px;
  }

  .stats {
    gap: 16px;
  }

  .card-actions {
    flex-wrap: wrap;
  }
}
</style>
