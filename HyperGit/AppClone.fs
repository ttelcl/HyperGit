module AppClone

open CommonTools
open ColorPrint
open System.IO
open HyperGitLib

type private CloneOptions = {
  CfgFile: string
}

let gitpath (path:string) =
  if path.Contains(' ') then
    "\"" + path.Replace('\\', '/') + "\""
  else
    path.Replace('\\', '/')    

let run args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "-f" :: cfgfile :: rest ->
      rest |> parseMore {o with CfgFile = cfgfile}
    | [] ->
      o
    | x :: _ ->
      failwithf $"Unrecognized argument '{x}'"
  let o = args |> parseMore {
    CfgFile = "repos.csv"
  }
  let hgc = new HyperGitConfig(o.CfgFile)
  for e in hgc.Entries do
    if e.IsEnabled then
      let repopath = e.RepoPath
      if repopath |> Directory.Exists then
        if verbose then
          cp $"\fk{e.Name}\f0 (exists)"
      else
        cp $"\fg{e.Name}\f0:"
        if e.BucketPath |> Directory.Exists |> not then
          cp $"  Creating \fy{e.BucketPath}\f0"
          Directory.CreateDirectory(e.BucketPath) |> ignore
        let needName =
          e.Renamed
        let bucket = e.BucketPath |> gitpath
        if needName then
          cp $"  \fwgit -C {bucket} clone --mirror --no-hardlinks {e.Origin} {e.Name}.git\f0"
        else
          cp $"  \fwgit -C {bucket} clone --mirror --no-hardlinks {e.Origin}\f0"
    else
      if verbose then
        cp $"\fk%35s{e.Name}  (inactive)"
  0

