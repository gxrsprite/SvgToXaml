# SvgToXaml
转换部分svg为xaml  
path → Path  
rect → Rectangle  
polygon → Polygon  
text → Label  
circle → Ellipse  
ellipse → Ellipse  
处理属性和部分样式  

可能可以支持的svg来源：  
www.iconfont.cn  
SymbolFactory  
AI导出必须使用内联样式  

使用方法：  
拖入svg  
打开转出来的xaml将Viewbox及子节点拷贝至你需要的地方，调整Grid的大小以便正好框住图形(目前没有遍历最大宽度和高度，未自动设置Grid的大小)
