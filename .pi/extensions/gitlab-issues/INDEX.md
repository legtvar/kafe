# GitLab Issues Extension - Documentation Index

Welcome to the GitLab Issues extension for the KAFE project! This index will help you navigate the documentation.

## 🚀 Getting Started

**New to this extension? Start here:**

1. **[QUICKSTART.md](./QUICKSTART.md)** - Get up and running in 5 minutes
   - Quick examples
   - Common workflows
   - Troubleshooting tips

2. **[README.md](./README.md)** - Complete user guide
   - Feature overview
   - Detailed usage instructions
   - Authentication setup

## 📚 User Documentation

### For Daily Use

- **[QUICKSTART.md](./QUICKSTART.md)** - Fast reference for common tasks
- **[README.md](./README.md)** - Full feature documentation

### Reference Materials

- **`.pi/skills/gitlab-issues.md`** - Skill reference (for Pi's context)
- **[manifest.json](./manifest.json)** - Extension metadata and capabilities

## 🔧 Technical Documentation

### For Developers

- **[IMPLEMENTATION.md](./IMPLEMENTATION.md)** - Implementation details
  - Architecture overview
  - Technical decisions
  - Future enhancements
  
- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - System architecture
  - Component diagrams
  - Data flow charts
  - API specifications

### Source Code

- **[index.ts](./index.ts)** - Main extension implementation (~400 lines)
  - Well-commented TypeScript
  - Follows Pi extension patterns
  - Based on GitHub issue autocomplete example

## 📋 Documentation Map

```
User Flow:
  INDEX.md (you are here)
    ├─→ QUICKSTART.md → Start using immediately
    └─→ README.md → Learn all features

Technical Flow:
  INDEX.md (you are here)
    ├─→ IMPLEMENTATION.md → How it works
    ├─→ ARCHITECTURE.md → System design
    └─→ index.ts → Source code

Reference:
  INDEX.md (you are here)
    ├─→ manifest.json → Extension metadata
    └─→ ../skills/gitlab-issues.md → Pi skill docs
```

## 🎯 Quick Navigation

### I want to...

#### Use the extension
→ Read [QUICKSTART.md](./QUICKSTART.md)

#### Understand all features
→ Read [README.md](./README.md)

#### Know how it works internally
→ Read [IMPLEMENTATION.md](./IMPLEMENTATION.md)

#### See the system architecture
→ Read [ARCHITECTURE.md](./ARCHITECTURE.md)

#### Modify or extend the code
→ Read [IMPLEMENTATION.md](./IMPLEMENTATION.md) + [index.ts](./index.ts)

#### Check extension metadata
→ Read [manifest.json](./manifest.json)

#### Help Pi understand the extension
→ Read `.pi/skills/gitlab-issues.md`

## 📖 Documentation Summary

| Document | Audience | Length | Purpose |
|----------|----------|--------|---------|
| QUICKSTART.md | All users | Short | Get started fast |
| README.md | All users | Medium | Complete feature guide |
| IMPLEMENTATION.md | Developers | Long | Technical implementation |
| ARCHITECTURE.md | Architects/Devs | Long | System design & flows |
| index.ts | Developers | ~400 lines | Source code |
| manifest.json | Systems | Short | Extension metadata |
| skills/gitlab-issues.md | Pi AI | Medium | AI context & skills |

## 🔍 Search Tips

Looking for something specific? Try searching these documents:

- **"authentication"** - Token setup, security
- **"autocomplete"** - Issue reference completion
- **"tools"** - Available tool functions
- **"API"** - GitLab API integration
- **"error"** - Error handling, troubleshooting
- **"cache"** - Caching strategy, performance

## 🆘 Need Help?

1. Check [QUICKSTART.md](./QUICKSTART.md) troubleshooting section
2. Review [README.md](./README.md) for detailed explanations
3. Look at [IMPLEMENTATION.md](./IMPLEMENTATION.md) for technical details
4. Check the root [.PI-EXTENSION-NOTICE.md](../../.PI-EXTENSION-NOTICE.md)

## 📝 Version Information

- **Extension Version**: 1.0.0
- **Last Updated**: 2026-05-11
- **Compatible with**: Pi coding agent >= 1.0.0
- **Repository**: git@gitlab.fi.muni.cz:legtvar/kafe.git

## 🔄 Updates

To check for updates or contribute improvements:
1. Review [IMPLEMENTATION.md](./IMPLEMENTATION.md) for future enhancement ideas
2. Check the main project repository
3. Submit merge requests to the KAFE project

---

**Happy coding with GitLab issues integration! 🎉**
