<template>
  <div class="category-detail-panel">
    <!-- Placeholder khi chưa chọn danh mục -->
    <div v-if="!selectedCategory" class="placeholder">
      <div class="placeholder-icon">
        <i class="el-icon-folder"></i>
      </div>
      <div class="placeholder-text">
        <h3>Chọn một danh mục</h3>
        <p>Nhấp vào nút danh mục bên trái để xem thông tin chi tiết và danh sách bài viết</p>
      </div>
    </div>

    <!-- Hiển thị chi tiết khi chọn danh mục -->
    <div v-else class="category-details">
      <!-- Thông tin cơ bản của danh mục -->
      <div class="category-info">
        <div class="info-header">
          <div class="category-title">
            <h2>{{ selectedCategory.text }}</h2>
            <div class="category-actions">
              <el-button size="small" @click="createPost">
                <i class="el-icon-plus"></i> Tạo bài viết mới
              </el-button>
              <el-button size="small" @click="addSubCategory">
                <i class="el-icon-folder-add"></i> Thêm danh mục con
              </el-button>
              <el-button size="small" @click="editCategory">
                <i class="el-icon-edit"></i> Sửa danh mục
              </el-button>
            </div>
          </div>
        </div>

        <div class="info-stats">
          <div class="stat-item">
            <div class="stat-label">ID danh mục</div>
            <div class="stat-value">{{ selectedCategory.id }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Số lượng bài viết</div>
            <div class="stat-value">{{ selectedCategory.tags[0] }}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Thời gian tạo</div>
            <div class="stat-value">{{ formatDate(selectedCategory.createTime) }}</div>
          </div>
          <div class="stat-item" v-if="selectedCategory.updateTime">
            <div class="stat-label">Thời gian cập nhật</div>
            <div class="stat-value">{{ formatDate(selectedCategory.updateTime) }}</div>
          </div>
        </div>
      </div>

      <!-- Danh sách bài viết -->
      <div class="posts-section">
        <div class="section-header">
          <h3>Bài viết trong danh mục</h3>
          <div class="section-actions">
            <el-button
              size="mini"
              icon="el-icon-s-grid"
              :type="displayMode === 'card' ? 'primary' : 'default'"
              @click="displayMode = 'card'"
            ></el-button>
            <el-button
              size="mini"
              icon="el-icon-menu"
              :type="displayMode === 'icon' ? 'primary' : 'default'"
              @click="displayMode = 'icon'"
            ></el-button>
            <el-button size="mini" @click="refreshPosts">
              <i class="el-icon-refresh"></i> Làm mới
            </el-button>
          </div>
        </div>

        <div class="posts-content" v-loading="postsLoading">
          <!-- Danh sách bài viết -->
          <div v-if="categoryPosts.length > 0">
            <!-- Dạng thẻ -->
            <div v-if="displayMode === 'card'" class="posts-grid">
              <ArticleCard
                v-for="post in categoryPosts"
                :key="post.id"
                :post="post"
                @click="viewPost"
                @edit="editPost"
                @delete="deletePost"
              />
            </div>

            <!-- Dạng biểu tượng -->
            <div v-else-if="displayMode === 'icon'" class="posts-icon-grid">
              <ArticleIcon
                v-for="post in categoryPosts"
                :key="post.id"
                :post="post"
                @select="viewPost"
                @view="viewPost"
              />
            </div>
          </div>

          <!-- Trạng thái trống -->
          <div v-else-if="!postsLoading" class="posts-empty">
            <div class="empty-icon">
              <i class="el-icon-document"></i>
            </div>
            <div class="empty-text">
              <h4>Chưa có bài viết</h4>
              <p>Danh mục này chưa có bài viết, nhấn "Tạo bài viết" ở phía trên để bắt đầu sáng tác</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ArticleCard from './ArticleCard.vue'
import ArticleIcon from './ArticleIcon.vue'

export default {
  name: "CategoryDetailPanel",
  components: {
    ArticleCard,
    ArticleIcon
  },
  props: {
    selectedCategory: {
      type: Object,
      default: null
    },
    categoryPosts: {
      type: Array,
      default: () => []
    },
    postsLoading: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      displayMode: 'icon' // 'card' or 'icon'
    }
  },
  methods: {
    // Định dạng ngày
    formatDate(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
      });
    },

    // Tạo bài viết
    createPost() {
      this.$emit('create-post');
    },

    // Thêm danh mục con
    addSubCategory() {
      this.$emit('add-subcategory', this.selectedCategory);
    },

    // Sửa danh mục
    editCategory() {
      this.$emit('edit-category', this.selectedCategory);
    },

    // Làm mới danh sách bài viết
    refreshPosts() {
      this.$emit('refresh-posts');
    },

    // Xem bài viết
    viewPost(post) {
      this.$emit('view-post', post);
    },

    // Sửa bài viết
    editPost(post) {
      this.$emit('edit-post', post);
    },

    deletePost(post) {
      this.$emit('delete-post', post);
    },

    handleIconSelect(post) {
      // Ở chế độ biểu tượng, nhấp vào biểu tượng tương đương với xem bài viết
      this.viewPost(post);
    }
  }
}
</script>

<style scoped>
.category-detail-panel {
  height: 100%;
  background: #f8f9fa;
  overflow-y: auto;
}

/* Kiểu dáng placeholder */
.placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  text-align: center;
  color: #909399;
  padding: 40px;
}

.placeholder-icon {
  font-size: 64px;
  margin-bottom: 24px;
  opacity: 0.6;
}

.placeholder-text h3 {
  margin: 0 0 12px 0;
  font-size: 20px;
  font-weight: 500;
  color: #606266;
}

.placeholder-text p {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
}

/* Kiểu dáng chi tiết danh mục */
.category-details {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.category-info {
  background: #fff;
  border-bottom: 1px solid #ebeef5;
  padding: 24px;
  flex-shrink: 0;
}

.info-header {
  margin-bottom: 20px;
}

.category-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.category-title h2 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  color: #303133;
}

.category-actions {
  display: flex;
  gap: 8px;
}

.info-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 12px;
  color: #909399;
  font-weight: 500;
}

.stat-value {
  font-size: 16px;
  color: #303133;
  font-weight: 600;
}

/* Kiểu dáng danh sách bài viết */
.posts-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px 16px;
  background: #fff;
  border-bottom: 1px solid #ebeef5;
}

.section-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #303133;
}

.posts-content {
  flex: 1;
  padding: 16px 24px 24px;
  overflow-y: auto;
}

.posts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.posts-icon-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}

/* Kiểu dáng trạng thái trống */
.posts-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 300px;
  text-align: center;
  color: #909399;
}

.posts-empty .empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.6;
}

.posts-empty h4 {
  margin: 0 0 8px 0;
  font-size: 16px;
  font-weight: 500;
  color: #606266;
}

.posts-empty p {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
}

/* Thiết kế responsive */
@media (max-width: 768px) {
  .category-info {
    padding: 16px;
  }

  .category-title {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .category-actions {
    width: 100%;
    justify-content: flex-start;
    flex-wrap: wrap;
  }

  .info-stats {
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }

  .section-header {
    padding: 16px;
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .posts-content {
    padding: 12px 16px 16px;
  }

  .placeholder {
    padding: 20px;
  }

  .placeholder-icon {
    font-size: 48px;
    margin-bottom: 16px;
  }

  .placeholder-text h3 {
    font-size: 18px;
  }
}
</style>
