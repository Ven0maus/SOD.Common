# SOD.ZeroOverhead

A lightweight performance optimization mod that reduces unnecessary CPU work, minimizes per-frame overhead, and improves frame-time stability — without changing gameplay behavior.

---

## Overview

**ZeroOverhead** applies targeted runtime optimizations to high-frequency systems that commonly cause frame drops and stutter.  
The mod focuses on reducing redundant updates, caching expensive operations, and smoothing logic execution across frames.

It is designed to be:
- Safe
- Transparent
- Low-overhead
- Compatible with existing saves

---

## Features

- Reduces unnecessary `Update`-loop executions
- Optimizes high-cost runtime code paths
- Lowers CPU overhead in complex scenes
- Improves frame pacing and reduces micro-stutter
- No gameplay changes