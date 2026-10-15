<template>
  <div class="category-detail-container">
    <!-- Thông tin phần đầu -->
    <div class="detail-header">
      <div class="header-left">
        <el-button type="text" icon="el-icon-arrow-left" @click="goBack" class="back-button">
          Quay lại danh sách danh mục
        </el-button>
      </div>
      <div class="category-info">
        <div class="category-icon">
          <i class="el-icon-folder-opened"></i>
        </div>
        <div class="category-details">
          <h1 class="category-name">{{ category.name }}</h1>
          <p class="category-description">{{ category.description || 'Chưa có mô tả' }}</p>
          <div class="category-meta">
            <el-tag :type="category.visible ? 'success' : 'info'" size="small">
              {{ category.visible ? 'Hiển thị' : 'Ẩn' }}
            </el-tag>
            <span class="meta-item">Tổng số bài viết: {{ totalArticleCount }}</span>
            <span class="meta-item">Đã tải: {{ categoryArticles.length }}</span>
          </div>
        </div>
      </div>
      <div class="header-actions">
        <el-button type="primary" icon="el-icon-edit" @click="editCategory">
          Sửa danh mục
        </el-button>
        <el-button type="success" icon="el-icon-check" @click="saveChanges" :loading="saving" :disabled="!hasChanges">
          Lưu thay đổi
        </el-button>
      </div>
    </div>

    <!-- Vùng nội dung chính -->
    <div class="detail-content">
      <!-- Bên trái: danh sách bài viết có thể thêm -->
      <div class="articles-panel left-panel">
        <div class="panel-header">
          <h3>Bài viết có thể thêm</h3>
          <div class="panel-controls">
            <el-input v-model="articleSearch" placeholder="Tìm bài viết..." prefix-icon="el-icon-search" size="small"
              style="width: 200px;" clearable @input="searchArticles" />
            <el-cascader v-model="filterCategoryId" :options="categoriesTree" filterable clearable
              placeholder="Lọc danh mục" size="small" style="width: 150px; margin-left: 10px;" :props="{
                checkStrictly: true,
                expandTrigger: 'hover',
                emitPath: false,
              }" @change="filterArticles">
            </el-cascader>
            <el-button size="small" icon="el-icon-refresh" @click="refreshAvailableArticles"
              :loading="loadingAvailableArticles">
              Làm mới
            </el-button>
          </div>
        </div>

        <div class="articles-list" v-loading="loadingAvailableArticles">
          <div v-for="article in availableArticles" :key="article.id" class="article-item"
            :class="{ 'disabled': isArticleInCategory(article.id) }">
            <div class="article-info">
              <h4 class="article-title">{{ article.title }}</h4>
              <p class="article-meta">
                <span>{{ formatDate(article.creationTime) }}</span>
                <span class="article-status">{{ article.status }}</span>
              </p>
            </div>
            <div class="article-actions">
              <el-button v-if="!isArticleInCategory(article.id)" size="mini" type="primary" icon="el-icon-plus"
                @click="addArticleToCategory(article)">
                Thêm
              </el-button>
              <span v-else class="already-added">Đã thêm</span>
            </div>
          </div>

          <!-- Trạng thái trống -->
          <div v-if="availableArticles.length === 0 && !loadingAvailableArticles" class="empty-articles">
            <i class="el-icon-document-remove"></i>
            <p>Chưa có bài viết nào có thể thêm</p>
          </div>

          <!-- Tải thêm -->
          <div v-if="availableArticles.length > 0 && hasMoreAvailableArticles" class="load-more-section">
            <el-button type="text" @click="loadMoreAvailableArticles" :loading="loadingAvailableArticles"
              icon="el-icon-more">
              Tải thêm
            </el-button>
          </div>
        </div>
      </div>

      <!-- Bên phải: danh sách bài viết của danh mục hiện tại -->
      <div class="articles-panel right-panel">
        <div class="panel-header">
          <h3>Bài viết của danh mục hiện tại</h3>
          <div class="panel-controls">
            <el-input v-model="categoryArticleSearch" placeholder="Tìm bài viết của danh mục hiện tại..." prefix-icon="el-icon-search"
              size="small" style="width: 200px;" clearable />
            <el-button size="small" type="primary" icon="el-icon-refresh" @click="refreshCategoryArticles"
              :loading="loadingCategoryArticles">
              Làm mới
            </el-button>
            <el-button size="small" type="danger" icon="el-icon-delete" @click="removeSelectedArticles"
              :disabled="newlyAddedSelectedCount === 0">
              Gỡ bài mới thêm ({{ newlyAddedSelectedCount }})
            </el-button>
          </div>
        </div>

        <div class="articles-list" v-loading="loadingCategoryArticles">
          <div v-for="article in filteredCategoryArticles" :key="article.id" class="article-item current-category">
            <div class="article-checkbox">
              <el-checkbox v-model="selectedArticles" :label="article.id" />
            </div>
            <div class="article-info">
              <h4 class="article-title">{{ article.title }}</h4>
              <p class="article-meta">
                <span>{{ formatDate(article.creationTime) }}</span>
                <span class="article-status">{{ article.slug }}</span>
              </p>
            </div>
            <div class="article-actions">
              <el-button v-if="isNewlyAddedArticle(article.id)" size="mini" type="danger" icon="el-icon-minus" @click="removeArticleFromCategory(article)">
                Gỡ
              </el-button>
              <span v-else class="cannot-remove">Đã lưu</span>
            </div>
          </div>

          <!-- Trạng thái trống -->
          <div v-if="filteredCategoryArticles.length === 0 && !loadingCategoryArticles" class="empty-articles">
            <i class="el-icon-document-add"></i>
            <p>Danh mục này chưa có bài viết</p>
            <p class="empty-tip">Chọn bài viết từ danh sách bên trái để thêm vào danh mục này</p>
          </div>

          <!-- Tải thêm -->
          <div v-if="categoryArticles.length > 0 && hasMoreCategoryArticles" class="load-more-section">
            <el-button type="text" @click="loadMoreCategoryArticles" :loading="loadingCategoryArticles"
              icon="el-icon-more">
              Tải thêm ({{ categoryArticles.length }}/{{ totalArticleCount }})
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- Hộp thoại sửa danh mục -->
    <add-category-dialog ref="editDialog" @onUpdateSucceed="onCategoryUpdated" />
  </div>
</template>

<script>
import AddCategoryDialog from "./AddDialog.vue";

export default {
  name: "CategoryDetail",
  components: {
    AddCategoryDialog
  },
  data() {
    return {
      // Dữ liệu cơ bản
      category: {},
      allCategories: [],
      categoriesTree: [],

      // Dữ liệu bài viết
      availableArticles: [],
      categoryArticles: [],
      selectedArticles: [],

      // Tìm kiếm và lọc
      articleSearch: '',
      categoryArticleSearch: '',
      filterCategoryId: '',

      // Dữ liệu phân trang
      availableArticlesPage: 1,
      availableArticlesPageSize: 20,
      categoryArticlesPage: 1,
      categoryArticlesPageSize: 20,
      totalArticleCount: 0,
      totalAvailableCount: 0,

      // Trạng thái tải
      loadingAvailableArticles: false,
      loadingCategoryArticles: false,
      saving: false,

      // Theo dõi thay đổi
      pendingChanges: {
        added: []
      }
    }
  },
  computed: {
    hasChanges() {
      return this.pendingChanges.added.length > 0
    },
    hasMoreAvailableArticles() {
      return this.availableArticles.length < this.totalAvailableCount
    },
    hasMoreCategoryArticles() {
      return this.categoryArticles.length < this.totalArticleCount
    },
    filteredCategoryArticles() {
      if (!this.categoryArticleSearch) {
        return this.categoryArticles
      }
      return this.categoryArticles.filter(article =>
        article.title.toLowerCase().includes(this.categoryArticleSearch.toLowerCase())
      )
    },
    // Tính số lượng bài viết mới thêm có thể gỡ
    newlyAddedSelectedCount() {
      return this.selectedArticles.filter(articleId => 
        this.pendingChanges.added.includes(articleId)
      ).length
    }
  },
  async mounted() {
    await this.initializeData()
  },
  methods: {
    async initializeData() {
      const categoryId = this.$route.params.id
      if (!categoryId) {
        this.$message.error('ID danh mục không được để trống')
        this.goBack()
        return
      }

      try {
        // Tải dữ liệu song song
        await Promise.all([
          this.loadCategory(categoryId),
          this.loadAllCategories(),
          this.loadCategoriesTree(),
          this.loadAvailableArticles(),
          this.loadCategoryArticles(categoryId)
        ])
      } catch (error) {
        console.error('Khởi tạo dữ liệu thất bại:', error)
        this.$message.error('Tải dữ liệu thất bại: ' + error.message)
      }
    },

    async loadCategory(categoryId) {
      try {
        const res = await this.$api.category.get(categoryId)
        this.category = res.data
      } catch (error) {
        console.error('Tải thông tin danh mục thất bại:', error)
        throw error
      }
    },

    async loadAllCategories() {
      try {
        const res = await this.$api.category.getAll()
        this.allCategories = res.data
      } catch (error) {
        console.error('Tải danh sách danh mục thất bại:', error)
        throw error
      }
    },

    async loadCategoriesTree() {
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

      try {
        const res = await this.$api.category.getNodes()
        this.categoriesTree = mapNodes(res.data)
      } catch (error) {
        console.error('Tải cây danh mục thất bại:', error)
        throw error
      }
    },

    async loadAvailableArticles(page = 1, append = false) {
      this.loadingAvailableArticles = true
      try {
        const res = await this.$api.blogPost.getList(
          null,                                    // isPublish - Đã xuất bản hay chưa (null nghĩa là không lọc)
          null,                                    // status - Trạng thái bài viết (null nghĩa là không lọc)
          this.filterCategoryId || null,           // categoryId - Lọc theo ID danh mục
          this.articleSearch,                      // search - Từ khóa tìm kiếm
          null,                                      // sortBy - Cách sắp xếp (chuỗi rỗng nghĩa là sắp xếp mặc định)
          page,                                    // page - Số trang
          this.availableArticlesPageSize           // pageSize - Số lượng mỗi trang
        )

        console.log('loadAvailableArticles response:', res)

        const articles = Array.isArray(res.data) ? res.data : []
        if (append) {
          this.availableArticles = [...this.availableArticles, ...articles]
        } else {
          this.availableArticles = articles
        }

        if (res.pagination && typeof res.pagination.totalItemCount === 'number') {
          this.totalAvailableCount = res.pagination.totalItemCount
        } else {
          this.totalAvailableCount = this.availableArticles.length
        }

        this.availableArticlesPage = page
      } catch (error) {
        console.error('Tải bài viết khả dụng thất bại:', error)
        this.$message.error('Tải bài viết khả dụng thất bại: ' + error.message)
      } finally {
        this.loadingAvailableArticles = false
      }
    },

    async loadCategoryArticles(categoryId, page = 1, append = false) {
      this.loadingCategoryArticles = true
      try {
        const res = await this.$api.category.getPosts(
          categoryId, page, this.categoryArticlesPageSize
        )

        console.log('loadCategoryArticles response:', res)

        const articles = Array.isArray(res.data) ? res.data : []
        if (append) {
          this.categoryArticles = [...this.categoryArticles, ...articles]
        } else {
          this.categoryArticles = articles
        }

        if (res.pagination && typeof res.pagination.totalItemCount === 'number') {
          this.totalArticleCount = res.pagination.totalItemCount
        } else {
          this.totalArticleCount = this.categoryArticles.length
        }

        this.categoryArticlesPage = page

        console.log('categoryArticles count:', this.categoryArticles.length, 'total:', this.totalArticleCount)
      } catch (error) {
        console.error('Tải bài viết của danh mục thất bại:', error)
        this.$message.error('Tải bài viết của danh mục thất bại: ' + error.message)
      } finally {
        this.loadingCategoryArticles = false
      }
    },

    // Phương thức làm mới
    async refreshAvailableArticles() {
      this.availableArticlesPage = 1
      await this.loadAvailableArticles(1, false)
    },

    async refreshCategoryArticles() {
      this.categoryArticlesPage = 1
      await this.loadCategoryArticles(this.category.id, 1, false)
    },

    // Phương thức tải thêm
    async loadMoreAvailableArticles() {
      if (!this.loadingAvailableArticles && this.hasMoreAvailableArticles) {
        const nextPage = this.availableArticlesPage + 1
        await this.loadAvailableArticles(nextPage, true)
      }
    },

    async loadMoreCategoryArticles() {
      if (!this.loadingCategoryArticles && this.hasMoreCategoryArticles) {
        const nextPage = this.categoryArticlesPage + 1
        await this.loadCategoryArticles(this.category.id, nextPage, true)
      }
    },

    // Tìm kiếm và lọc
    async searchArticles() {
      this.availableArticlesPage = 1
      await this.loadAvailableArticles(1, false)
    },

    async filterArticles() {
      this.availableArticlesPage = 1
      await this.loadAvailableArticles(1, false)
    },

    // Quản lý bài viết
    isArticleInCategory(articleId) {
      return this.categoryArticles.some(article => article.id === articleId) ||
        this.pendingChanges.added.includes(articleId)
    },

    // Kiểm tra xem có phải bài viết mới thêm lần này không
    isNewlyAddedArticle(articleId) {
      return this.pendingChanges.added.includes(articleId)
    },

    addArticleToCategory(article) {
      if (this.isArticleInCategory(article.id)) return

      this.categoryArticles.push(article)
      this.pendingChanges.added.push(article.id)

      this.$message.success(`Bài viết đã thêm: ${article.title}`)
    },

    removeArticleFromCategory(article) {
      // Chỉ cho phép gỡ các bài viết mới thêm lần này
      if (!this.pendingChanges.added.includes(article.id)) {
        this.$message.warning('Chỉ có thể gỡ các bài viết mới thêm lần này')
        return
      }

      const index = this.categoryArticles.findIndex(a => a.id === article.id)
      if (index > -1) {
        this.categoryArticles.splice(index, 1)
        
        // Xóa khỏi danh sách thêm
        const addIndex = this.pendingChanges.added.indexOf(article.id)
        if (addIndex > -1) {
          this.pendingChanges.added.splice(addIndex, 1)
        }
      }

      // Xóa khỏi danh sách đã chọn
      const selectedIndex = this.selectedArticles.indexOf(article.id)
      if (selectedIndex > -1) {
        this.selectedArticles.splice(selectedIndex, 1)
      }

      this.$message.success(`Đã gỡ bài viết mới thêm: ${article.title}`)
    },

    removeSelectedArticles() {
      const newlyAddedSelected = this.selectedArticles.filter(articleId => 
        this.pendingChanges.added.includes(articleId)
      )
      
      if (newlyAddedSelected.length === 0) {
        this.$message.warning('Vui lòng chọn bài viết mới thêm cần gỡ')
        return
      }

      this.$confirm(`Bạn có chắc muốn gỡ ${newlyAddedSelected.length} bài viết mới thêm này?`, 'Xác nhận gỡ', {
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        type: 'warning'
      }).then(() => {
        newlyAddedSelected.forEach(articleId => {
          const article = this.categoryArticles.find(a => a.id === articleId)
          if (article) {
            this.removeArticleFromCategory(article)
          }
        })
        this.$message.success(`Đã gỡ ${newlyAddedSelected.length} bài viết mới thêm`)
      })
    },

    // Lưu thay đổi
    async saveChanges() {
      if (!this.hasChanges) {
        this.$message.info('Không có thay đổi nào cần lưu')
        return
      }

      this.saving = true
      try {
        // Chỉ xử lý bài viết mới thêm: đặt danh mục cho bài viết mới thêm
        const addedPromises = this.pendingChanges.added.map(articleId => {
          const article = this.availableArticles.find(a => a.id === articleId)
            || this.categoryArticles.find(a => a.id === articleId)
          if (!article) return Promise.resolve()
          return this.$api.blogPost.update({
            ...article,
            categoryId: this.category.id
          })
        })

        await Promise.all(addedPromises)

        this.pendingChanges = { added: [] }
        this.$message.success('Lưu thành công')

        // Làm mới dữ liệu hai bên
        await Promise.all([
          this.refreshCategoryArticles(),
          this.refreshAvailableArticles()
        ])
      } catch (error) {
        console.error('Lưu thất bại:', error)
        this.$message.error('Lưu thất bại:' + error.message)
      } finally {
        this.saving = false
      }
    },

    // Thao tác khác
    editCategory() {
      this.$refs.editDialog.edit(this.category)
    },

    onCategoryUpdated(updatedCategory) {
      this.category = { ...this.category, ...updatedCategory }
      this.$message.success('Cập nhật thông tin danh mục thành công')
    },

    goBack() {
      this.$router.back()
    },

    formatDate(dateString) {
      if (!dateString) return ''
      return new Date(dateString).toLocaleDateString('zh-CN')
    }
  }
}
</script>

<style scoped>
.category-detail-container {
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;
}

.detail-header {
  background: white;
  border-radius: 8px;
  padding: 24px;
  margin-bottom: 20px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.back-button {
  font-size: 14px;
  color: #409eff;
  padding: 0;
}

.back-button:hover {
  color: #66b1ff;
}

.category-info {
  display: flex;
  align-items: center;
  flex: 1;
  margin: 0 20px;
}

.category-icon {
  font-size: 48px;
  color: #409eff;
  margin-right: 20px;
}

.category-name {
  font-size: 28px;
  font-weight: 600;
  color: #303133;
  margin: 0 0 8px 0;
}

.category-description {
  font-size: 16px;
  color: #606266;
  margin: 0 0 12px 0;
}

.category-meta {
  display: flex;
  align-items: center;
  gap: 16px;
}

.meta-item {
  font-size: 14px;
  color: #909399;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.detail-content {
  display: flex;
  gap: 20px;
  height: calc(100vh - 200px);
}

.articles-panel {
  flex: 1;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 20px;
  border-bottom: 1px solid #f0f0f0;
  background: #fafbfc;
  border-radius: 8px 8px 0 0;
}

.panel-header h3 {
  margin: 0 0 16px 0;
  font-size: 18px;
  font-weight: 600;
  color: #303133;
}

.panel-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.articles-list {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.article-item {
  display: flex;
  align-items: center;
  padding: 16px;
  margin-bottom: 12px;
  background: #f8f9fa;
  border-radius: 8px;
  transition: all 0.2s ease;
  cursor: pointer;
}

.article-item:hover {
  background: #e3f2fd;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.article-item.disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background: #f5f5f5;
}

.article-item.current-category {
  background: #fff7e6;
  border: 1px solid #ffd591;
}

.article-checkbox {
  margin-right: 12px;
}

.article-info {
  flex: 1;
  min-width: 0;
}

.article-title {
  font-size: 16px;
  font-weight: 500;
  color: #303133;
  margin: 0 0 8px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.article-meta {
  font-size: 14px;
  color: #909399;
  margin: 0;
  display: flex;
  gap: 16px;
  align-items: center;
}

.article-status {
  padding: 2px 8px;
  background: #e1f3d8;
  color: #67c23a;
  border-radius: 4px;
  font-size: 12px;
}

.article-actions {
  margin-left: 12px;
}

.already-added {
  font-size: 12px;
  color: #909399;
  padding: 4px 8px;
}

.empty-articles {
  text-align: center;
  padding: 60px 20px;
  color: #909399;
}

.empty-articles i {
  font-size: 64px;
  margin-bottom: 16px;
  display: block;
  opacity: 0.5;
}

.empty-tip {
  font-size: 14px;
  margin-top: 8px;
  opacity: 0.8;
}

.load-more-section {
  text-align: center;
  padding: 20px;
  border-top: 1px solid #f0f0f0;
  margin-top: 16px;
}

.load-more-section .el-button {
  font-size: 14px;
  color: #409eff;
}

.load-more-section .el-button:hover {
  color: #66b1ff;
}

/* Thiết kế responsive */
@media (max-width: 1200px) {
  .detail-content {
    flex-direction: column;
    height: auto;
  }

  .articles-panel {
    height: 500px;
  }
}

@media (max-width: 768px) {
  .category-detail-container {
    padding: 12px;
  }

  .detail-header {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }

  .category-info {
    margin: 0;
    justify-content: center;
    text-align: center;
  }

  .header-actions {
    justify-content: center;
  }

  .panel-controls {
    flex-direction: column;
    align-items: stretch;
  }

  .article-item {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .article-actions {
    margin-left: 0;
    text-align: center;
  }
}
</style>
