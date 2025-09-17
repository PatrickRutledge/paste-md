# Test Samples for paste-md

Copy any of these samples to test the paste-md functionality:

---

## Sample 1: Basic Formatting

# Main Title
## Subtitle
**Bold text** and *italic text* and ***both***

---

## Sample 2: Code Block (Should have gray background)

Here's a Python function:

```python
def calculate_sum(a, b):
    """Calculate sum of two numbers"""
    result = a + b
    print(f"The sum is: {result}")
    return result
```

And some `inline code` text.

---

## Sample 3: Lists

### Unordered List:
- First item
- Second item
  - Nested item 1
  - Nested item 2
- Third item

### Ordered List:
1. Step one
2. Step two
   1. Sub-step A
   2. Sub-step B
3. Step three

---

## Sample 4: Links and Blockquotes

[Visit GitHub](https://github.com)

> This is a blockquote
> It can span multiple lines
>
> > And can be nested

---

## Sample 5: Table

| Feature | Status | Notes |
|---------|--------|-------|
| Headers | ✅ | Working |
| Code Blocks | ✅ | Gray background |
| Lists | ✅ | Formatted |
| Tables | ✅ | With borders |

---

## Sample 6: Complex Document

# Project Documentation

## Introduction
This is a **comprehensive** test of the *paste-md* functionality.

## Features
- **Markdown parsing** with `Markdig`
- Code blocks with syntax highlighting:

```javascript
function greet(name) {
    console.log(`Hello, ${name}!`);
    return true;
}
```

## Installation Steps
1. Download the installer
2. Run as Administrator
3. Follow the setup wizard
4. Restart Windows Explorer

## Configuration
Edit the `config.json` file:

```json
{
  "version": "1.0.0",
  "settings": {
    "enabled": true
  }
}
```

> **Note:** Always backup your configuration before making changes.

## Conclusion
The paste-md extension makes working with Markdown a breeze!

---

## Testing Instructions

1. **Select and copy** any sample above
2. **Open** Word, OneNote, Outlook, or PowerPoint
3. **Right-click** where you want to paste
4. **Choose** "Paste as Rendered Markdown"
5. **Verify** the formatting matches expectations:
   - Headers are bold and sized appropriately
   - Code blocks have gray background (#f6f8fa)
   - Code uses monospace font (Consolas)
   - Links are blue and underlined
   - Lists are properly indented
   - Tables have borders