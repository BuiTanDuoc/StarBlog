# StarBlog

![](./docs/images/logo.jpg)


<p align="center">
<a href="https://dotnet.microsoft.com/download/dotnet/6.0"><img src="https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 6"></a>
<a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Vue3-4FC08D?style=flat-square&logo=vue.js" alt="Vue 3"></a>
<a href="https://blog.deali.cn"><img src="https://img.shields.io/badge/Demo-Online-brightgreen?style=flat-square" alt="Online Demo"></a>
<a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Repository-181717?style=flat-square&logo=github" alt="Admin Repository"></a>
</p>

## 📝 Introduction

StarBlog is a modern blog system built with .NET 6 and ASP.NET Core. It supports Markdown article imports and adheres to RESTful API standards. The frontend is developed using Vue and ElementUI. This project serves as an excellent entry-level learning resource for .NET Core; it is accompanied by a series of development notes documenting the entire process of building the system from scratch, helping learners understand the development workflow of .NET Core projects. **Live Demo**: [https://blog.deali.cn](https://blog.deali.cn)

**Admin Dashboard Project**: [https://github.com/Deali-Axy/StarBlog-Admin](https://github.com/Deali-Axy/StarBlog-Admin)

**AI Article Publishing Tool**: [https://github.com/star-blog/starblog-publisher](https://github.com/star-blog/starblog-publisher)

## 📫 Project Motivation

With so many online blogging platforms available, **why develop my own blog?**

I have been tinkering with blogs for quite some time—starting with WordPress, then moving on to static site generators like Hexo and Hugo hosted on GitHub—and I have constantly been experimenting and tweaking things along the way. I experimented with various solutions to better address my needs, but ultimately decided to invest the time to build my own system; you can view the specific details in my [development notes](https://www.cnblogs.com/deali/p/16104454.html).

- I couldn't find an existing online blogging platform that satisfied me.
- The writing experience on online platforms doesn't compare to using Typora locally.
- Writing for platforms like WeChat Official Accounts often distracts me with the need to make content "catchy" rather than focusing on the article itself.
- Hosting my own website gives me full control and ensures the content isn't polluted by unwanted ads or inappropriate information often found on third-party platforms.

## ✨ Features

- **Markdown Support**: (**Core Feature 💡**) Batch import of local Markdown articles; supports nested categories and automatic image importing/content conversion.
- **Article Management**: Supports uploading and automatically importing individual articles (including image attachments) as packages.
- **Photography Showcase**: Supports batch uploading of local photography and automatically reads EXIF ​​data.
- **Customizable Homepage**: Configurable blog homepage supporting charts/random image displays and pinned articles, images, or categories.
- **Theme Switching**: The frontend supports switching between multiple theme styles.
- **Friendly Links**: Comprehensive link management, including support for user-submitted link requests.
- **Traffic Statistics**: High-performance access logging, statistics, and data visualization.
- **Random Image API**: Includes a random image API; random images are used as default article cover images.
- **Comment System**: Built-in commenting for registered users and guests, featuring automatic filtering and moderation.
- **RSS Subscription**: Supports RSS feeds.

## 🌟 StarBlog Ecosystem

StarBlog is more than just a blogging system; it is evolving into a complete blogging ecosystem that includes various supporting tools and projects:

### StarBlog Publisher

[StarBlog Publisher](https://github.com/star-blog/starblog-publisher) is a professional article publishing tool designed specifically for StarBlog, offering a more convenient publishing method than traditional package uploads. - **Markdown Support**: Supports previewing and publishing articles in Markdown format.
- **Intuitive Interface**: Offers a user-friendly interface for easily managing and publishing blog content.
- **AI Assistance**: Integrates various leading large AI models (including OpenAI's ChatGPT, Anthropic's Claude, DeepSeek, etc.).
- **Cross-Platform**: Built using C# and .NET 8.0; runs on Windows, macOS, and Linux.

### StarBlog Admin

[StarBlog Admin](https://github.com/Deali-Axy/StarBlog-Admin) is the administrative backend for StarBlog. Developed using Vue and ElementUI, it provides comprehensive blog content management capabilities.

### StarBlogHub

A decentralized blog aggregation platform that allows various personal blogs to connect and share traffic. ## 🏗️ Architecture

### Tech Stack

#### Backend

##### Core Framework & Infrastructure
- **Web Framework**: ASP.NET Core - Cross-platform, high-performance .NET web application framework
- **Monitoring & Debugging**: Rin - Real-time inspection tool for ASP.NET Core applications

##### Data Access & Processing
- **Primary ORM**: FreeSql - High-performance ORM framework supporting multiple databases
- **Secondary ORM**: Entity Framework Core - Microsoft's official ORM framework
- **Object Mapping**: AutoMapper - Library for simplifying object-to-object mapping

##### API & Authentication
- **API Documentation**: Swagger/OpenAPI (Swashbuckle.AspNetCore) - Tool for automatic RESTful API documentation generation
- **Authentication Mechanism**: JWT (JSON Web Token) - Secure cross-domain authentication solution
- **Search Engine Support**: RobotsTxtCore - Manages search engine crawler access policies

##### Content Processing
- **Markdown Engine**: [Markdig](https://github.com/xoofx/markdig) - High-performance Markdown processor
- **Image Processing**: ImageSharp - Cross-platform image processing library
- **Pagination Component**: X.PagedList - Efficient data pagination solution
- **RSS Support**: System.ServiceModel.Syndication - RSS content aggregation tool

##### Communication & Utilities
- **Email Service**: MailKit - Cross-platform email client library

#### Frontend
- **Blog Frontend**: Bootstrap + Vue + ElementUI + editor.md + bootswatch
- **Admin Backend**: Vue + Vuex + Vue Router + ElementUI + SCSS

### Project Structure

```
StarBlog/
├── src/
│   ├── StarBlog.Api/       # Pure WebAPI (HTTP layer/Composition Root: Controllers + Middlewares + DI)
│   ├── StarBlog.Application/ # Application Layer (Use case orchestration/Application services/DTOs/Background tasks, etc., consumed by StarBlog.Api)
│   ├── StarBlog.Infrastructure/ # General Infrastructure (Diagnostics/Miscellaneous Capabilities)
│   ├── StarBlog.Content/   # Content processing (Markdown/ToC/Import handling, etc.)
│   ├── StarBlog.Data/      # Data models and data access layer (EF Core + FreeSql)
│   └── StarBlog.Web/       # Main Web application (MVC/Razor + API; legacy path, currently unchanged)
├── tools/
│   ├── MarkdownImporter/   # Blog post import CLI tool (formerly StarBlog.Migrate)
│   ├── DataProc/           # Data processing tools (access log completion/image optimization/summaries, etc.)
│   └── BlogImageOptimizer/ # Image optimization tool
└── demo/                   # Demo/experimental projects (isolated from the main product)
```

### Layering and Responsibilities (DDD/Clean Architecture Approach)

This decomposition aligns more closely with "DDD layering principles/Clean Architecture": decoupling the HTTP adapter layer from "Use Case orchestration (Application Layer)," making it easier to evolve business logic and infrastructure without affecting external APIs.

#### Current Layering (Implemented)

```
StarBlog.Api (Presentation / Delivery)
└─ Depends on → StarBlog.Application (Application Layer)
└─ Depends on → StarBlog.Data / StarBlog.Infrastructure / StarBlog.Content
```

- **StarBlog.Api**
- Responsible solely for HTTP: Controllers, middleware (Auth/Swagger/HealthChecks), DI composition root, routing, CORS policies, etc.
- Does not house business rules or complex orchestration; maintains "thin controllers."
- **StarBlog.Application**
- Houses the application layer: Application services (Use Case orchestration), DTOs/ViewModels, query parameter objects, background tasks (Outbox/VisitRecord Worker, etc.). 
- Aims for "stable business behavior + testability" while avoiding dependency on StarBlog.Api (unidirectional dependency flow).

#### Is this DDD?

- **Aligns with the "layered architecture" principle of DDD**: The current `StarBlog.Application` roughly corresponds to the DDD Application Layer, while `StarBlog.Api` corresponds to the Presentation/Interface Adapter layer.
- **However, it is not yet "full DDD"** (the current state is "layered architecture" without an explicit domain layer):
- There is no independent `StarBlog.Domain` module to represent aggregates, value objects, domain services, domain events, etc.
- `StarBlog.Data.Models` still carries the bulk of the domain model (leaning more towards "persistence models" or "anemic models"), and repository abstractions are not yet fully decoupled from the infrastructure.

#### Recommended Evolution Path (Optional)

If you wish to move closer to DDD (or the classic layering of Clean Architecture) in the future, you can gradually introduce the following:

```
StarBlog.Api → StarBlog
```
#### StarBlog.Web
	```
	npm install     # hoặc pnpm install
	npx gulp move
	```
