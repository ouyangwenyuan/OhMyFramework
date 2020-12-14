# 介绍和使用

ohmyframework 文档，

## 主要功能

1. 自动初始化framework，ModuleManager 管理所有扩展模块，实现模块扩展接口，将自动加入管理列表
2. 编辑器框架，实现编辑器扩展接口，将自动显示在编辑器面版中。
3. 日志类管理日志和常用类的扩展类，配置类，常量类，非常简单的异步任务执行类，基于类型的事件类。

## 模块扩展

using OhMyFramework.Core;

继承AModule 抽象类 或者 实现 IModule 接口。


## 编辑器扩展

using OhMyFramework.Editor;

继承 ModuleEditor<T>或者 实现 IModuleEditor 接口。
