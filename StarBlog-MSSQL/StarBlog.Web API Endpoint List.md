# StarBlog.Web API Endpoint Inventory

This document outlines the existing API endpoints in the `src/StarBlog.Web` project. They are categorized by purpose—"Public," "Backend," and "Site Resources"—and the document identifies the minimal set of endpoints required for the Next.js frontend migration. ## Conventions and Notes

- API endpoints are primarily located at: `src/StarBlog.Web/Apis/**`
- Site MVC pages are primarily located at: `src/StarBlog.Web/Controllers/**` (These are not WebAPI endpoints, though some serve as "resource-outputting endpoints"—see "Site Resources")
- API routing relies mainly on attribute routing, with most routes using the `Api/[controller]` prefix
- Authentication method: JWT Bearer (`[Authorize]` / `[AllowAnonymous]`)
- Response style: A global `ResponseWrapperFilter` is registered; most responses are wrapped in a unified structure, though a few actions still return "raw" data (requiring frontend attention or future standardization)

## Public (Anonymous access allowed)

### Blog

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Get pinned post | GET | `Api/Blog/Top` | Anonymous | Returns raw `Post?` |
| Get featured posts list | GET | `Api/Blog/Featured` | Anonymous | Returns raw `List<Post>` |
| Get blog overview | GET | `Api/Blog/Overview` | Anonymous | Returns raw `BlogOverview` |
| Get status list | GET | `Api/Blog/GetStatusList` | Anonymous | Returns raw `List<string?>` |
| Paginated posts list | GET | `Api/BlogPost` | Anonymous (Action-level) | Returns `ApiResponsePaged<Post>` |
| Post details (by ID) | GET | `Api/BlogPost/{id}` | Anonymous (Action-level) | Returns `ApiResponse<Post>` |
| Post details (by slug) | GET | `Api/BlogPost/slug/{slug}` | Anonymous (Action-level) | Returns `ApiResponse<Post>` |

### Categories

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Category Tree (Nodes) | GET | `Api/Category/Nodes` | Anonymous (Action attribute) | Returns raw `List<CategoryNode>?` |
| All Categories | GET | `Api/Category/All` | Anonymous (Action attribute) | Returns raw `List<Category>` |
| Paginated Category List | GET | `Api/Category?page=1&pageSize=10` | Anonymous (Action attribute) | Returns `ApiResponsePaged<Category>` |
| Category Details | GET | `Api/Category/{id:int}` | Anonymous (Action attribute) | Returns `ApiResponse<Category>` |
| Category Word Cloud | GET | `Api/Category/WordCloud` | Anonymous (Action attribute) | Returns raw `List<object>` |

### Photography/Photos

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Paginated Photo List | GET | `Api/Photo?page=1&pageSize=10` | Anonymous (Action attribute) | Returns `ApiResponsePaged<Photo>` |
| Photo Details | GET | `Api/Photo/{id}` | Anonymous (Action attribute) | Returns `ApiResponse<Photo>` |
| Get Thumbnail | GET | `Api/Photo/{id}/Thumb?width=300&quality=85` | Anonymous (Action attribute) | Returns image stream (`image/jpeg`) |

### Comments

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Paginated Comment List | GET | `Api/Comment` | Anonymous | Returns `ApiResponsePaged<Comment>` |
| Get Post Comments | GET | `Api/Comment/GetAll?postId=...` | Anonymous | Returns raw `List<Comment>?` |
| Send Email OTP | GET | `Api/Comment/GetEmailOtp?email=...` | Anonymous | Returns `ApiResponse` |
| Get anonymous user (email + OTP) | GET | `Api/Comment/GetAnonymousUser?email=...&otp=...` | Anonymous | Returns `ApiResponse` |
| Add comment | POST | `Api/Comment` | Anonymous | Returns `ApiResponse<Comment>` |

### Friendly Links (for display)

| Function | Method | Path | Auth | Remarks |
|---|---:|---|---|---|
| List friendly links | GET | `Api/Link` | Login required (Controller `[Authorize]`) | Currently implemented for backend use; for frontend display, it is recommended to split this into a public read-only endpoint later |

### Themes

| Function | Method | Path | Auth | Remarks |
|---|---:|---|---|---|
| List themes | GET | `Api/Theme` | Anonymous | Returns raw `List<Theme>` |

## Backend (Login Required)

### Authentication/Identity

| Function | Method | Path | Auth | Remarks |
|---|---:|---|---|---|
| Login | POST | `Api/Auth` (or `Api/Auth/Login`) | Anonymous | Returns `ApiResponse<LoginToken>` |
| Get current user | GET | `Api/Auth` (or `Api/Auth/GetUser`) | Login required | Returns `ApiResponse<User>` |

### Configuration (Config)

| Function | Method | Path | Auth | Remarks |
|---|---:|---|---|---|
| Get all configurations | GET | `Api/Config` | Login required | Returns raw `List<ConfigItem>` |
| Get config by key | GET | `Api/Config/{key}` | Login required | Returns `ApiResponse<ConfigItem>` |
| Add configuration | POST | `Api/Config` | Login required | Returns `ApiResponse<ConfigItem>` |
| Update configuration | PUT | `Api/Config/{key}` | Login required | Returns `ApiResponse<ConfigItem>` |
| Delete configuration | DELETE | `Api/Config/{key}` | Login required | Returns `ApiResponse` |

### Blog Post (BlogPost)

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Create post | POST | `Api/BlogPost` | Login required (Controller `[Authorize]`) | Returns `ApiResponse<Post>` |
| Update post | PUT | `Api/BlogPost/{id}` | Login required | Returns `ApiResponse<Post>` |
| Delete post | DELETE | `Api/BlogPost/{id}` | Login required | Returns `ApiResponse` |
| Upload post image | POST | `Api/BlogPost/{id}/UploadImage` | Login required | Returns `ApiResponse` |
| Get post image list | GET | `Api/BlogPost/{id}/Images` | Login required | Returns `ApiResponse<List<string>>` |
| Set/Unset featured | POST | `Api/BlogPost/{id}/SetFeatured` / `Api/BlogPost/{id}/CancelFeatured` | Login required | Returns `ApiResponse` or `ApiResponse<FeaturedPost>` |
| Set as top post | POST | `Api/BlogPost/{id}/SetTop` | Login required | Returns `ApiResponse<TopPost>` |

### Category (Category)

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| Create category | POST | `Api/Category` | Login required (Controller `[Authorize]`) | Returns `ApiResponse<Category>` |
| Update category | PUT | `Api/Category/{id:int}` | Login required | Returns `ApiResponse<Category>` |
| Delete category | DELETE | `Api/Category/{id:int}` | Login required | Returns `ApiResponse` |
| Set/Unset featured | POST | `Api/Category/{id:int}/SetFeatured` / `Api/Category/{id:int}/CancelFeatured` | Login required |  |
| Set Visible/Invisible | POST | `Api/Category/{id:int}/SetVisible` / `Api/Category/{id:int}/SetInvisible` | Login Required |  |

### Featured

| Function | Method | Path | Auth | Notes |
|---|---:|---|---|---|
| List/Details of Featured Posts | GET | `Api/FeaturedPost` / `Api/FeaturedPost/{id:int}` | Anonymous (Action-annotated) | Admin-managed but publicly readable |
| Add/Delete Featured Post | POST/DELETE | `Api/FeaturedPost?postId=...` / `Api/FeaturedPost/{id:int}` | Login Required |  |
| List/Details of Featured Categories | GET | `Api/FeaturedCategory` / `Api/FeaturedCategory/{id:int}` | Anonymous (Action-annotated) |  |
| Add/Delete Featured Category | POST/DELETE | `Api/FeaturedCategory` / `Api/FeaturedCategory/{id:int}` | Login Required |  |
| List/Details of Featured Photos | GET | `Api/FeaturedPhoto` / `Api/FeaturedPhoto/{id:int}` | Anonymous (Action-annotated) |  |
| Add/Remove Featured Photo | POST/DELETE | `Api/FeaturedPhoto` / `Api/FeaturedPhoto/{id:int}` | Login Required |  |

### Photography/Photos (Management)

| Function | Method | Path | Authentication | Notes |
|---|---:|---|---|---|
| Add Photo | POST | `Api/Photo` | Login Required (Controller `[Authorize]`) | `multipart/form-data` |
| Update Photo | PUT | `Api/Photo/{id}` | Login Required |  |
| Delete Photo | DELETE | `Api/Photo/{id}` | Login Required |  |
| Set/Unset as Featured | POST | `Api/Photo/{id}/SetFeatured` / `Api/Photo/{id}/CancelFeatured` | Login Required |  |
| Rebuild Data | POST | `Api/Photo/ReBuildData` | Login Required |  |
| Batch Import | POST | `Api/Photo/BatchImport` | Login Required |  |

### Links (Management)

| Function | Method | Path | Authentication | Notes |
|---|---:|---|---|---|
| Get List/Details | GET | `Api/Link` / `Api/Link/{id:int}` | Login Required (Controller `[Authorize]`) |  |
| Add | POST | `Api/Link` | Login Required | Action returns the `Link` object directly |
| Update/Delete | PUT/DELETE | `Api/Link/{id:int}` | Login Required |  |

### Link Exchange Requests (Review)

| Function | Method | Path | Authentication | Notes |
|---|---:|---|---|---|
| List/Details | GET | `Api/LinkExchange` / `Api/LinkExchange/{id:int}` | Login Required (Controller `[Authorize]`) |  |
| Accept/Reject | POST | `Api/LinkExchange/{id:int}/Accept` / `Api/LinkExchange/{id:int}/Reject` | Login Required |  |
| Delete | DELETE | `Api/LinkExchange/{id:int}` | Login Required |  |

### Visit Statistics (VisitRecord)

| Function | Method | Path | Authentication | Notes |
|---|---:|---|---|---|
| Visit Record Pagination/Details | GET | `Api/VisitRecord` / `Api/VisitRecord/{id:int}` | Login Required |  |
| Overview/Trends/Distribution/Top, etc. | GET | `Api/VisitRecord/*` | Login Required | Multiple endpoints, consolidated within a single controller |

### Runtime Dashboard

| Function | Method | Path | Authentication | Notes |
|---|---:|---|---|---|
| CLR Runtime Statistics | GET | `Api/Dashboard/ClrStats` | Login Required |  |

## Site Resources (Not JSON APIs, but function as "interface-style resources")

These endpoints are currently located within MVC Controllers and return XML or file streams. If the frontend is fully migrated to Next.js in the future, it is recommended that these be generated by Next.js (or handled by a dedicated API project). | Function | Method | Path | Location | Notes |
|---|---:|---|---|---|
| RSS Feed | GET | `/feed` | `Controllers/RssController` | `application/xml` |
| Sitemap Index | GET | `/sitemap-index.xml` | `Controllers/SitemapController` | `application/xml` |
| Sitemap (Articles, etc.) | GET | `/sitemap.xml` | `Controllers/SitemapController` | `application/xml` |
| Image Sitemap | GET | `/sitemap-images.xml` | `Controllers/SitemapController` | `application/xml` |

## Minimum API Set Required for Next.js (Recommended)

Focusing on the goal of "getting Next.js up and running while covering key pages," ensure the following endpoints are stable and available:

### Articles and Homepage

- `GET Api/BlogPost`: Homepage/list page pagination
- `GET Api/BlogPost/slug/{slug}`: Article detail page (using `slug` for routing is recommended)
- `GET Api/Blog/Top`: Pinned/top posts for the homepage (optional)
- `GET Api/Blog/Featured`: Featured posts for the homepage (optional)
- `GET Api/Blog/Overview`: Site overview (optional)

### Categories and Navigation

- `GET Api/Category/Nodes`: Navigation/category tree (or sidebar)
- `GET Api/Category/All`: Collection of all categories (optional)

### Comments

- `GET Api/Comment/GetAll?postId=...`: List of comments for an article detail page
- `POST Api/Comment`: Submit a comment
- `GET Api/Comment/GetEmailOtp` + `GET Api/Comment/GetAnonymousUser`: Identity handling for anonymous comments (if the Next.js frontend retains the current logic)

### Photography

- `GET Api/Photo`: List of photographs
- `GET Api/Photo/{id}`: Photograph details
- `GET Api/Photo/{id}/Thumb`: Thumbnail for lists

### Theme/Appearance

- `GET Api/Theme`: List of themes (if required by the frontend)

## SEO (Recommended to migrate to Next.js-based generation)

- `/feed`, `/sitemap*.xml`

## Known Issues for Future Refinement (Optional)

- CORS: Previous multiple calls to `WithOrigins(...)` may have resulted in only the last-specified origin taking effect; this has been consolidated into a single call in `Program.cs`.
- Response Wrapping: There is a mix of "raw" responses and `ApiResponse`-wrapped responses; please be mindful of this when making calls from Next.js (it is recommended to standardize DTOs and response models in the future).