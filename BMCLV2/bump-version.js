const fs = require('fs')

const file = './Properties/AssemblyInfo.cs'

const context = fs.readFileSync(file, 'utf8')
fs.writeFileSync(file, context.replace(/\[assembly: AssemblyVersion\("\d+\.\d+\.\d+\.(\d+)"\)]/, (str, build) => {
  const buildNumber = parseInt(build, 10)
  return str.replace(build, (buildNumber + 1).toString())
}))
