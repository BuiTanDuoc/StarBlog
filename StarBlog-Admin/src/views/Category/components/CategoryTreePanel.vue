<template>
  <div class="category-tree-panel">
    <div class="panel-header">
      <h2 class="panel-title">Quản lý danh mục</h2>
      <el-button type="primary" size="small" @click="handleAdd">
        <i class="el-icon-plus"></i> Thêm danh mục
      </el-button>
    </div>

    <div class="search-section">
      <el-input
        v-model="searchText"
        placeholder="Tìm danh mục..."
        prefix-icon="el-icon-search"
        size="small"
        clearable
      />
    </div>

    <div class="tree-actions">
      <el-button-group size="mini">
        <el-button @click="expandAll">
          <i class="el-icon-arrow-down"></i> Mở rộng tất cả
        </el-button>
        <el-button @click="collapseAll">
          <i class="el-icon-arrow-up"></i> Thu gọn tất cả
        </el-button>
      </el-button-group>
    </div>

    <div class="tree-container">
      <el-tree
        ref="categoryTree"
        :data="treeData"
        :props="treeProps"
        :filter-node-method="filterNode"
        node-key="id"
        default-expand-all
        :expand-on-click-node="false"
        @node-click="handleNodeClick"
        v-loading="loading"
      >
        <div
          slot-scope="{ node, data }"
          class="tree-node"
          :class="{ 'is-selected': selectedNodeId === data.id }"
        >
          <div class="node-content">
            <i class="node-icon el-icon-folder" v-if="hasChildren(data)"></i>
            <i class="node-icon el-icon-price-tag" v-else></i>
            <span class="node-label">{{ node.label }}</span>
            <span class="node-count" v-if="getCategoryPostsCount(data)">({{ getCategoryPostsCount(data) }} bài)</span>
            <span class="sub-category-count" v-if="hasChildren(data)">
              ({{ getChildrenCount(data) }} danh mục con)
            </span>
          </div>

          <!-- Nút thao tác -->
          <div class="action-buttons">
            <el-button
              type="text"
              size="mini"
              icon="el-icon-plus"
              title="Thêm danh mục con"
              @click.stop="handleMenuCommand({ action: 'addSubCategory', data })"
            ></el-button>
            <el-button
              type="text"
              size="mini"
              icon="el-icon-document"
              title="Xem bài viết"
              @click.stop="handleMenuCommand({ action: 'viewPosts', data })"
            ></el-button>
            <el-button
              type="text"
              size="mini"
              icon="el-icon-edit"
              title="Sửa danh mục"
              @click.stop="handleMenuCommand({ action: 'editCategory', data })"
            ></el-button>
            <el-button
              type="text"
              size="mini"
              icon="el-icon-delete"
              title="Xóa danh mục"
              class="danger-item"
              @click.stop="handleMenuCommand({ action: 'deleteCategory', data })"
            ></el-button>
          </div>
        </div>
      </el-tree>

      <div v-if="!loading && treeData.length === 0" class="empty-state">
        <i class="el-icon-folder-opened"></i>
        <p>Chưa có dữ liệu danh mục</p>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "CategoryTreePanel",
  props: {
    treeData: {
      type: Array,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    },
    selectedCategory: {
      type: Object,
      default: null
    }
  },
  data() {
    return {
      searchText: '',
      selectedNodeId: null,
      treeProps: {
        children: 'nodes',
        label: 'text'
      }
    }
  },
  watch: {
    searchText(val) {
      this.$refs.categoryTree.filter(val);
    },
    selectedCategory: {
      handler(newVal) {
        this.selectedNodeId = newVal ? newVal.id : null;
      },
      immediate: true
    }
  },
  methods: {
    // Lọc nút
    filterNode(value, data) {
      if (!value) return true;
      return data.text.indexOf(value) !== -1;
    },

    // Mở rộng tất cả các nút
    expandAll() {
      this.$nextTick(() => {
        const tree = this.$refs.categoryTree;
        if (tree && this.treeData.length > 0) {
          // Đệ quy mở rộng tất cả các nút
          const expandAllNodes = (nodes) => {
            nodes.forEach(nodeData => {
              const node = tree.getNode(nodeData);
              if (node && !node.expanded && node.childNodes.length > 0) {
                node.expanded = true;
              }
              if (nodeData.nodes && nodeData.nodes.length > 0) {
                expandAllNodes(nodeData.nodes);
              }
            });
          };
          
          setTimeout(() => {
            expandAllNodes(this.treeData);
          }, 50);
        }
      });
    },

    // Thu gọn tất cả các nút
    collapseAll() {
      this.$nextTick(() => {
        const tree = this.$refs.categoryTree;
        if (tree) {
          // Đệ quy thu gọn tất cả các nút
          const collapseAllNodes = (nodes) => {
            nodes.forEach(nodeData => {
              const node = tree.getNode(nodeData);
              if (node && node.expanded) {
                node.expanded = false;
              }
              if (nodeData.nodes && nodeData.nodes.length > 0) {
                collapseAllNodes(nodeData.nodes);
              }
            });
          };
          
          setTimeout(() => {
            collapseAllNodes(this.treeData);
          }, 50);
        }
      });
    },

    // Sự kiện nhấp vào nút
    handleNodeClick(data) {
      this.$emit('node-click', data);
    },

    // Xử lý lệnh menu
    handleMenuCommand(command) {
      this.$emit('menu-command', command);
    },

    // Thêm danh mục
    handleAdd() {
      this.$emit('add-category');
    },

    // Kiểm tra nút có nút con hay không
    hasChildren(data) {
      return data.nodes && data.nodes.length > 0;
    },

    // Lấy số lượng danh mục con
    getChildrenCount(data) {
      return data.nodes ? data.nodes.length : 0;
    },

    getCategoryPostsCount(data) {
      if (!data.tags || data.tags.length === 0) {
        return 0;
      }
      return data.tags[0];
    },
  }
}
</script>

<style scoped>
.category-tree-panel {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
  border-right: 1px solid #e6e6e6;
}

.panel-header {
  padding: 16px;
  border-bottom: 1px solid #e6e6e6;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.search-section {
  padding: 16px;
  border-bottom: 1px solid #e6e6e6;
}

.tree-actions {
  padding: 12px 16px;
  border-bottom: 1px solid #e6e6e6;
  display: flex;
  gap: 8px;
}

.tree-container {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
  position: relative;
}

.el-tree {
  height: 100%;
}

/* Kiểu dáng nút cây */
.tree-node {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 8px;
  margin: 2px 0;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.tree-node:hover {
  background-color: #f5f7fa;
}

.tree-node.is-selected {
  background-color: #ecf5ff;
  border-left: 3px solid #409eff;
}

.node-content {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
  overflow: hidden;
}

.node-icon {
  font-size: 14px;
  color: #909399;
  flex-shrink: 0;
}

.node-label {
  flex: 1;
  font-size: 14px;
  color: #303133;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.node-count {
  font-size: 12px;
  color: #909399;
  flex-shrink: 0;
}

.sub-category-count {
  font-size: 12px;
  color: #999;
  flex-shrink: 0;
  margin-left: 4px;
}

/* Kiểu dáng nút thao tác */
.action-buttons {
  display: flex;
  align-items: center;
  padding-left: 20px;
  gap: 4px;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.tree-node:hover .action-buttons {
  opacity: 1;
}

.action-buttons .el-button {
  padding: 4px;
  font-size: 14px;
}

.action-buttons .el-button.danger-item {
  color: #f56c6c;
}

.action-buttons .el-button.danger-item:hover {
  color: #f89898;
}

/* Kiểu dáng trạng thái trống */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #909399;
}

.empty-state i {
  font-size: 48px;
  margin-bottom: 16px;
  color: #c0c4cc;
}

.empty-state p {
  font-size: 14px;
  margin: 0;
}

/* Element UI Kiểu dáng tùy chỉnh cho thành phần cây */
::v-deep .el-tree {
  background: transparent;
}

::v-deep .el-tree-node {
  position: relative;
}

::v-deep .el-tree-node__content {
  padding: 0;
  height: auto;
  background: transparent;
}

::v-deep .el-tree-node__content:hover {
  background: transparent;
}

::v-deep .el-tree-node__expand-icon {
  padding: 6px;
  color: #c0c4cc;
  transition: transform 0.3s ease;
}

::v-deep .el-tree-node__expand-icon.expanded {
  transform: rotate(90deg);
}

::v-deep .el-tree-node__expand-icon.is-leaf {
  color: transparent;
  cursor: default;
}

/* Hiệu ứng mở rộng/thu gọn */
::v-deep .el-tree-node__children {
  transition: all 0.3s ease;
}

/* Thiết kế responsive */
@media (max-width: 768px) {
  .panel-header {
    padding: 12px;
  }

  .search-section {
    padding: 12px;
  }

  .tree-actions {
    padding: 8px 12px;
  }

  .tree-node {
    padding: 6px 12px;
  }
}
</style>
