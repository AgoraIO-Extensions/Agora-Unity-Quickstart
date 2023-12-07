import sys
import os
import re
import shutil

root = sys.argv[1]
print ("allowUnsafeCode: root_dir " + root)


def get_all_files(target_dir):
    files = []
    list_files = os.listdir(target_dir)
    for i in range(0, len(list_files)):
        each_file = os.path.join(target_dir, list_files[i])
        if os.path.isdir(each_file):
            files.extend(get_all_files(each_file))
        elif os.path.isfile(each_file):
            files.append(each_file)
    return files

def replace_key_word_in_path(file_path, key_word, replace_word, suffix):
    files = get_all_files(file_path)
    for i in range(0, len(files)):
        file_name = files[i]
        if file_name.endswith(suffix):
            # print(file_name)
            f = open(file_name, 'r', encoding='UTF-8')
            content = f.read()
            content = content.replace(key_word, replace_word)
            f.close()
            f = open(file_name, 'w')
            f.write(content)
            f.close()


replace_key_word_in_path(root, "<AllowUnsafeBlocks>False</AllowUnsafeBlocks>", "<AllowUnsafeBlocks>True</AllowUnsafeBlocks>", ".csproj")
replace_key_word_in_path(root,"<AllowUnsafeBlocks>false</AllowUnsafeBlocks>","<AllowUnsafeBlocks>True</AllowUnsafeBlocks>", ".csproj")
replace_key_word_in_path(os.path.join(root, "ProjectSettings"), "allowUnsafeCode: 0", "allowUnsafeCode: 1", ".asset")