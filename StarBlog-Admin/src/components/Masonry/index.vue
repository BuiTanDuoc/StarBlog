<template>
  <div class="masonry" :style="masonryStyle">
    <slot></slot>
  </div>
</template>

<script>
export default {
  name: 'Masonry',
  props: {
    columnWidth: { type: Number, default: 260 },
    gap: { type: Number, default: 16 },
  },
  computed: {
    masonryStyle() {
      return {
        columnWidth: this.columnWidth + 'px',
        columnGap: this.gap + 'px',
        '--masonry-gap': this.gap + 'px',
      }
    }
  }
}
</script>

<style scoped>
.masonry {
  /* Trình duyệt tự động tính số cột theo column-width */
  column-fill: balance;
}

/* Giúp mỗi phần tử con trong cột không bị ngắt, và lấp đầy chiều rộng cột */
.masonry > * {
  break-inside: avoid;
  -webkit-column-break-inside: avoid;
  page-break-inside: avoid;
  display: inline-block;
  width: 100%;
  margin-bottom: var(--masonry-gap);
}
</style>