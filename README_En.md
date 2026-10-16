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

If you wish to move closer to DDD (or the classic layered structure of Clean Architecture) in the future, you can gradually introduce the following:

```
StarBlog.Api → StarBlog.Application → StarBlog.Domain
↘  (Interfaces) ↙
StarBlog.Infrastructure / StarBlog.Data (Implementations)
```

Core Principle: Dependencies point inward (layers containing more stable business rules should not depend on layers that are more prone to change). HTTP, databases, and third-party services are implemented and swappable at the outer layers.

## 🚀 Quick Start

### Prerequisites

- **.NET 9 SDK**
- **Node.js v18** or higher, along with **npm/yarn** (for managing front-end assets)

### Build Steps

1. **Clone the Repository**

```bash
git clone https://github.com/Deali-Axy/StarBlog.git
cd StarBlog
```

2. **Prepare Front-end Assets**

This project uses NPM and Gulp to manage static front-end files. It requires Node.js v18 or higher. For more details, see: [ASP.NET Core Development Notes: Managing Front-end Static Files with NPM and Gulp](https://www.cnblogs.com/deali/p/15905760.html).

```bash
cd src/StarBlog.Web
npm i -g bower
npm install  # or yarn
npm install --global gulp-cli
gulp move
gulp min
```

**Note**: This project depends on the [bootstrap5-treeview](https://www.npmjs.com/package/bootstrap5-treeview) component, which uses [Bower](https://bower.io/) for its build process. Please install Bower first (`npm i -g bower`); otherwise, the `npm install` step will fail.

3. **Run the Project**

Open the solution in Visual Studio or Rider, set `StarBlog.Web` as the startup project, and run it. Solution Overview:
- `StarBlog.sln`: Main product project (`src`)
- `StarBlog.Tools.sln`: Tools project (`tools`)
- `StarBlog.Demo.sln`: Demo/experimental project (`demo`)

To get started quickly, this project uses an SQLite database by default. Most features utilize FreeSQL as the ORM; you can run the project directly without any additional configuration, as FreeSQL will automatically generate the necessary database schema.

4. **Access Log Database Synchronization**

To optimize performance, the access log module in StarBlog uses a separate database managed by EFCore. For more details, see: [StarBlog Extra (1): New Access Statistics, Asynchronous Queues, and Database Sharding](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)

Unlike FreeSQL, EFCore does not automatically generate the database schema, so you must synchronize the database manually. It defaults to using SQLite, though you can switch to MySQL or PostgreSQL if preferred.

First, install the EFCore CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

Synchronize the database (Windows 10+)

```powershell
cd src/StarBlog.Data
$env:CONNECTION_STRING = "Data Source=..\StarBlog.Web\app.log.db"
dotnet ef database update
```

Synchronize the database (Linux/macOS)

```bash
cd StarBlog.Data
export CONNECTION_STRING="Data Source=../StarBlog.Web/app.log.db"
dotnet ef database update
```

### Initialization

After launching the StarBlog project for the first time, visit `/Home/Init` to perform initialization tasks, such as creating an administrator account. You will then be able to use this account to log in to the management dashboard.

**Note**: The initialization process can only be performed once. For further details, please refer to [StarBlog - (16) New Features (Monitoring/Statistics/Configuration/Initialization)](https://www.cnblogs.com/deali/p/16523157.html). ### Configuration

#### Email Configuration

StarBlog’s "Friendly Links" (blogroll) and comment system both utilize email functionality. For more details, see: [StarBlog - (28) Developing APIs for Friendly Links](https://blog.deali.cn/Blog/Post/f55c8c7610706503)

Please add a file named `appsettings.email.json` to the project root directory. Using an Outlook email account as an example, the configuration should look like this:

```json
"EmailAccountConfig": {
"Host": "smtp-mail.outlook.com",
"Port": 587,
"FromUsername": "your-email@outlook.com",
"FromPassword": "your-email-password",
"FromAddress": "your-email@outlook.com"
}
```

#### Sensitive Word Detection

StarBlog uses the DFA (Deterministic Finite Automaton) algorithm to detect sensitive words in comments. To enable this, you must place a sensitive word database file named `words.json` within the `StarBlog.Web` project directory.

To maintain a civil and harmonious online environment, this file is not included in the open-source repository; you will need to source it yourself.

The required format is as follows:

```json
[
{
"Id": 1,
"Value": "sensitive-word-example",
"Tag": "violence"
},
{
"Id": 2,
"Value": "another-sensitive-word",
"Tag": "advertisement"
}
]
```

> If you require this for learning or testing purposes, you can send a private message with the keyword "badwords" to the official WeChat account "程序设计实验室" (Programming Design Lab) to obtain it.
>
>**Sensitive Word List Disclaimer**
>
>This sensitive word list is provided solely for learning, research, and testing purposes. **It is strictly prohibited to use this list for any commercial, illegal, or other activities that may violate local laws or regulations.**
>
>Please note:
>
>- The content of this list is gathered from various sources and **does not represent the position or views of the StarBlog project or its author.**
>- **Users are prohibited from distributing, sharing, reselling, or publicly publishing any part or the entirety of this sensitive word list in any form.** >- Users must comply with all applicable laws and regulations; **any legal liability arising from use shall be borne solely by the user.**
>- For compliance and security reasons, the content of the word library (lexicon) has been encrypted. Users assume all risks associated with decryption and subsequent use.
>
>**By using this software, you acknowledge that you have read and agreed to the full terms of this disclaimer.**

## Deployment

StarBlog supports various deployment methods. The simplest approach is to publish the application in "self-contained" mode and then run it directly on your server.

```bash
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
```

Once published, upload the files from the `publish` directory to your server and execute the **StarBlog.Web** file.

For more information on deployment options, please refer to: [StarBlog - (31) Publishing and Deployment](https://blog.deali.cn/p/starblog-31)


## 📚 Development Notes

This project is accompanied by a series of development notes that document the entire process of building this blog system from scratch. These notes also serve as an introductory tutorial for .NET Core development.

> PS: As the project code is updated frequently, please use the **v1.0.0 version** when following the development note series.
>
> Clone command:
>
> ```bash
> git clone --branch v1.0.0 --single-branch https://github.com/Deali-Axy/StarBlog.git
> ```

- [Building a Blog with .NET Core: StarBlog - (1) Why Build Your Own Blog?] ](https://www.cnblogs.com/deali/p/16104454.html)
- [Developing a Blog Project with .NET Core: StarBlog - (2) Environment Setup and Project Creation](https://www.cnblogs.com/deali/p/16172342.html)
- [Developing a Blog Project with .NET Core: StarBlog - (3) Model Design](https://www.cnblogs.com/deali/p/16180920.html)
- [Developing a Blog Project with .NET Core: StarBlog - (4) Batch Importing Markdown Blog Posts](https://www.cnblogs.com/deali/p/16211720.html)
- [Developing a Blog Project with .NET Core: StarBlog - (5) Getting Started with the Web Project](https://www.cnblogs.com/deali/p/16276448.html)
- [Developing a Blog Project with .NET Core: StarBlog - (6) Page Development: Blog Post List](https://www.cnblogs.com/deali/p/16286780.html)
- [Developing a Blog Project with .NET Core: StarBlog - (7) Page Development: Article Detail Page](https://www.cnblogs.com/deali/p/16293309.html)
- [Developing a Blog Project with .NET Core: StarBlog - (8) Displaying Hierarchical Category Structures](https://www.cnblogs.com/deali/p/16307604.html)
- [Developing a Blog Project with .NET Core: StarBlog - (9) Batch Importing Images](https://www.cnblogs.com/deali/p/16328825.html)
- [Developing a Blog Project with .NET Core: StarBlog - (10) - [Image Waterfall Layout](https://www.cnblogs.com/deali/p/16335162.html)
- [Developing a Blog Project with .NET Core: StarBlog - (11) Implementing Visit Statistics](https://www.cnblogs.com/deali/p/16349155.html)
- [Developing a Blog Project with .NET Core: StarBlog - (12) Dynamic Compilation of Razor Pages](https://www.cnblogs.com/deali/p/16391656.html)
- [Developing a Blog Project with .NET Core: StarBlog - (13) Adding a "Friendly Links" Feature](https://www.cnblogs.com/deali/p/16421699.html)
- [Developing a Blog Project with .NET Core: StarBlog - (14) Implementing Theme Switching](https://www.cnblogs.com/deali/p/16441294.html)
- [Developing a Blog Project with .NET Core: StarBlog - (15) Generating Images of Random Dimensions](https://www.cnblogs.com/deali/p/16457314.html)
- [Developing a Blog Project with .NET Core: StarBlog - (16) New Features (Monitoring/Statistics/Configuration/Initialization)](https://www.cnblogs.com/deali/p/16523157.html)
- [Developing a Blog Project with .NET Core: StarBlog - (17) Automatically Downloading External Images from Articles](https://www.cnblogs.com/deali/p/16586437.html)
- [Developing a Blog Project with .NET Core: StarBlog - (18) Implementing Local Typora Article Packaging and Uploading](https://www.cnblogs.com/deali/p/16758878.html)
- [Developing a Blog Project with .NET Core: StarBlog - (19) Exploring Markdown Rendering Solutions](https://www.cnblogs.com/deali/p/16834452.html)
- [Developing a Blog Project with .NET Core: StarBlog - (20) Optimizing Image Display](https://www.cnblogs.com/deali/p/16929677.html)
- [Developing a Blog Project with .NET Core: StarBlog - (21) Getting Started with RESTful API Development](https://www.cnblogs.com/deali/p/16989798.html)
- [Developing a Blog Project with .NET Core: StarBlog - (22) Developing APIs for Blog Articles](https://www.cnblogs.com/deali/p/16991279.html)
- [Developing a Blog Project with .NET Core: StarBlog - (23) - [Article List API: Pagination, Filtering, Searching, and Sorting](https://www.cnblogs.com/deali/p/16992573.html)
- [Developing a Blog Project with .NET Core: StarBlog - (24) Standardizing API Response Formats](https://www.cnblogs.com/deali/p/16995384.html)
- [Developing a Blog Project with .NET Core: StarBlog - (25) Image API and File Uploads](https://www.cnblogs.com/deali/p/16999818.html)
- [Developing a Blog Project with .NET Core: StarBlog - (26) Integrating Swagger for API Documentation](https://www.cnblogs.com/deali/p/17093390.html)
- [Developing a Blog Project with .NET Core: StarBlog - (27) Securing APIs with JWT](https://blog.sblt.deali.cn:9000/Blog/Post/541b8beae183d29e)
- [Developing a Blog Project with .NET Core: StarBlog - (28) Developing APIs for Friendly Links](https://www.cnblogs.com/deali/p/starblog-28.html)
- [Developing a Blog Project with .NET Core: StarBlog - (29) Implementing RSS Subscription Functionality](https://www.cnblogs.com/deali/p/17501704.html)
- [Developing a Blog Project with .NET Core: StarBlog - (30) Implementing a Comment System](https://www.cnblogs.com/deali/p/17910094.html)
- [Developing a Blog Project with .NET Core: StarBlog - (31) Publishing and Deployment](https://www.cnblogs.com/deali/p/18011965)
- [Developing a Blog Project with .NET Core: StarBlog - (32) Phase 1 Complete](https://www.cnblogs.com/deali/p/18582026)
- ...

Side Stories (Extras):

- [StarBlog Extra (1): New Visit Statistics, Asynchronous Queues, and Sharded Database Storage](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)
- [StarBlog Extra (2): A Deep Dive into Markdig Source Code – Optimizing ToC Heading Extraction and Article Directory Tree Generation](https://blog.deali.cn/Blog/Post/d47403a6c7399c44)
- [StarBlog Extra (3): StarBlog Publisher – A Cross-Platform, One-Click Publishing Tool for Content Creation, Powered by DeepSeek](https://blog.deali.cn/Blog/Post/211884ccc25b02e2)


## 📷 Screenshots

### Blog Homepage

![](./docs/images/home.png)

### Article List

![](./docs/images/post-list.jpg)

### Photography Page ~~（Though I haven't actually uploaded any photos yet!）~~

![](./docs/images/photos.png)

### Admin Dashboard

![](./docs/images/admin.png)

### Admin Article List

![](./docs/images/admin-posts.png)

### Article Editor

![](./docs/images/admin-post-edit.png)

### Admin Image List

![](./docs/images/admin-photos.png)

## 🔗 Follow Me

Official Account | QR Code
------- | ------
![](./docs/images/coding_lab_logo.jpg) | ![](./docs/images/coding_lab_qr_code.jpg)


## 🔄 Limitations of the Current Version

Since the launch of the StarBlog project, I have been continuously learning about ASP.NET Core... Compared to when this project was first developed, my familiarity with the framework has grown, and I have naturally come to recognize certain limitations in the existing code:

- The "Read" operation in CRUD should be implemented using the `PATCH` method.
- The `[HttpHead]` attribute should be added to `GET` endpoints to support the `HEAD` method.
- Input parameters for filtering and search endpoints should be trimmed.
- Instead of forcing all API return types to `ApiResponse`, the framework’s native `ActionResult` type should be retained to allow for greater functionality.
- While API return types were standardized, exception handling was not; a unified error-handling approach should be implemented using the `app.UseExceptionHandler` middleware (or alternatively, exception filters).
- Support for certain Markdown elements—such as Tables of Contents (TOC), mathematical formulas, code blocks, and images nested within tables—remains inadequate.

These issues will be addressed in the v2 release. ## 🚀 v2 Roadmap

I have outlined several new features and optimizations, though this is by no means an exhaustive list for the v2 release. If you have any great suggestions, feel free to share them in the comments!

### Frontend Refactoring

- Rebuild using Next.js, Remix, or Astro (specific tech stack TBD).
- Implement Markdown parsing using a Node.js-based stack.

### Admin Dashboard Refactoring

- Rebuild using a React-based tech stack.

### New Analytics Features

- Geographic data visualization.
- Search engine indexing analysis.
- Anti-scraping/bot protection.
- Article view count tracking.

### Article Editing

- Integrate a new Markdown editor (ideally a WYSIWYG-style editor, similar to Wagtail).
- Support for embedding richer content (e.g., videos) within articles.

### Reading Experience Optimization

- Replace the current Markdown rendering tool. (I am currently using a heavily modified fork of [editor.md](https://github.com/Deali-Axy/editor.md-ext); while it works, the project is quite old and no longer maintained. I am looking for a modern, well-maintained alternative.)

### Article Encryption

- Set a static password.
- Require users to follow the official WeChat account to obtain a dynamic password.

### New Search Functionality

- Implement a full-text search engine.
- Incorporate embeddings.

### AI Features

- Knowledge base.
- Conversational interface (chat).
- AI-generated article summaries.
- Automated commenting.

## 🙏 Acknowledgments

<p align="center">
<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.png" alt="JetBrains Rider" width="200">
</p>

<p align="center">
Special thanks to <a href="https://jb.gg/OpenSourceSupport">JetBrains Open Source Support</a> for providing a free license for the development tool, Rider.
</p>

#### StarBlog.Web
	```
	npm install     # hoặc pnpm install
	npx gulp move
	```
