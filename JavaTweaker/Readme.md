用法
=============
###1.编译
#####1)使用MCP或直接在classpath中添加launchwrapper均可
#####2)将编译产生的东西以标准zip直接打成jar包(可压缩)，无需混淆或添加META-INF
###2.使用
#####1)将要替换核心文件的class，或额外加入核心文件的class放入jar中
#####2)以库文件挂载方式挂载jar(注：顺序应该是在最前面)
#####3)如果库文件列表中没有launchwrapper库，则挂载launchwrapper库 (注：顺序应该是在次前面)
#####4)如果启动参数中没有指定'--tweakClass'的话，则指定'--tweakClass bmcl.Tweaker'，如果已经指定则不能变更内容
#####5)确保'mainClass'为'net.minecraft.launchwrapper.Launch'
###3.注
#####1)使用中的内容和forge启动几乎一致，详见forge的json
#####2)这个库文件不会影响forge和其他mod，只会在loadClass的时候动态替换掉核心文件中的内容，从而实现不修改核心文件本体
