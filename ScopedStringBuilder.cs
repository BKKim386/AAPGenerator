using System;
using System.Text;

namespace AAPathGenerator
{
	public class CodeGenStringBuilder
	{
		public class Scope : IDisposable
		{
			private CodeGenStringBuilder _generator;
		
			public Scope(CodeGenStringBuilder generator)
			{
				_generator = generator;
			}

			public void Write(string value)
			{
				_generator.Write(value);
			}

			public void WriteLine()
			{
				_generator.WriteLine();
			}

			public void WriteLine(string value)
			{
				_generator.WriteLine(value);
			}

			public void WriteConstVariable(string name, string value)
			{
				_generator.WriteLine($"public const string {name} = \"{value}\";");
			}
		
			public void Dispose()
			{
				_generator.EndScope();
			}
		}

		public CodeGenStringBuilder()
		{
			_stringBuilder = new StringBuilder();
		}

		private StringBuilder _stringBuilder;
		private int _tabCount;

		private void AppendTab()
		{
			for(int i = 0; i < _tabCount; ++i)
				_stringBuilder.Append("\t");
		}
		
		public void Write(string value, bool appendTab = true)
		{
			if (appendTab) AppendTab();
			
			_stringBuilder.Append($"{value}");
		}

		public void WriteLine()
		{
			_stringBuilder.AppendLine();
		}

		public void WriteLine(string value, bool appendTab = true)
		{
			if (appendTab) AppendTab();
			
			_stringBuilder.AppendLine(value);
		}

		public Scope StartNamespaceScope(string name)
		{
			var newScope = new Scope(this);
			newScope.WriteLine($"namespace {name}");
			newScope.WriteLine("{");
			++_tabCount;
			return newScope;
		}

		public Scope StartClassScope(string name)
		{
			var newScope = new Scope(this);

			newScope.WriteLine($"public static class {name}");
			newScope.WriteLine("{");
			++_tabCount;
			return newScope;
		}

		public void EndScope()
		{
			--_tabCount;
			AppendTab();
			_stringBuilder.AppendLine("}");
		}

		public override string ToString()
		{
			return _stringBuilder.ToString();
		}
	}
}