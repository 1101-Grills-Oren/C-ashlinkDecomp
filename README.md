# C-ashlinkDecomp
A C# variation of the Crashlink Haxe decompiler

Entrypoint (for compiling the decompiler): `HashlinkDecompiler.Test`
Method to add to the Crashlink commands object:
```
def savestatics(self, args: List[str]) -> None:
        import base64
        """saves the static parts of the bytecode to a file. `savestatics <destfolder>`"""
        strs=",".join([base64.b64encode(bytes(s,'utf-8')).decode('utf-8') for s in self.code.strings.value])
        doubles=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.floats])
        ints=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.ints])
        typs=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.types])
        from crashlink.core import VarInt
        from crashlink.core import DebugInfo
        from io import BytesIO
        if(DebugInfo().deserialise(BytesIO(self.code.functions[0].debuginfo.serialise()[0:4]),self.code.functions[0].nops.value).value!=self.code.functions[0].debuginfo.value):
            print("eror")
        for i in self.code.functions:
            i.serialise()
        print(self.code.functions[0].nassigns,self.code.functions[0].nops)
        print(*[int(i) for i in VarInt().serialise()])
        print(*[int(i) for i in self.code.functions[0].debuginfo.serialise()])
        
        fncs=",".join([base64.b64encode(b''.join([[(b"\1" if i.has_debug else b'\0') if i.has_debug!=None else b'\0'][0],VarInt([i.version if i.version!=None else 0][0]).serialise(),i.serialise()])).decode('utf-8') for i in self.code.functions])
        globalTypes=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.global_types])
        byte_s=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in [[] if self.code.bytes==None else self.code.bytes.value][0]])
        natives=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.natives])
        constants=",".join([base64.b64encode(i.serialise()).decode('utf-8') for i in self.code.constants])
        x=open(args[0],"w")
        x.write(strs+";"+doubles+";"+ints+";"+typs+";"+fncs+";"+globalTypes+";"+byte_s+";"+natives+";"+constants)
        x.close()
```
