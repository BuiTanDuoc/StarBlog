using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Application.Services;

/// <summary>
/// Liên kết bạn bè
/// </summary>
public class LinkService {
    private IBaseRepository<Link> _repo;

    public LinkService(IBaseRepository<Link> repo) {
        _repo = repo;
    }

    /// <summary>
    /// Lấy toàn bộ liên kết bạn bè
    /// </summary>
    /// <param name="onlyVisible">Chỉ lấy các liên kết đang hiển thị</param>
    public async Task<List<Link>> GetAll(bool onlyVisible = true) {
        return onlyVisible
            ? await _repo.Where(a => a.Visible).ToListAsync()
            : await _repo.Select.ToListAsync();
    }

    public async Task<Link?> GetById(int id) {
        return await _repo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Link?> GetByName(string name) {
        return await _repo.Where(a => a.Name == name).FirstAsync();
    }

    /// <summary>
    /// Kiểm tra id có tồn tại hay không
    /// </summary>
    public async Task<bool> HasId(int id) {
        return await _repo.Where(a => a.Id == id).AnyAsync();
    }

    public async Task<Link> AddOrUpdate(Link item) {
        return await _repo.InsertOrUpdateAsync(item);
    }

    public async Task<Link?> SetVisibility(int id, bool visible) {
        var item = await GetById(id);
        if (item == null) return null;
        item.Visible = visible;
        await _repo.UpdateAsync(item);
        return item;
    }

    public async Task<int> DeleteById(int id) {
        return await _repo.DeleteAsync(a => a.Id == id);
    }
}