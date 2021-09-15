import { useState } from 'react';
import { ErrorMessage } from '@hookform/error-message';
import clsx from 'clsx';
import PropTypes from 'prop-types';
import { CloudUploadIcon, XIcon, CheckIcon } from '@heroicons/react/outline';
import { useDropzone } from 'react-dropzone';
import { toast } from 'react-toastify';
import ErrorMessageTag from './ErrorMessage';
import { Label } from './TextInput';

export const LimitedTextarea = ({
  name,
  rows,
  placeholder,
  value,
  limit,
  onChange,
  inputRef,
  register,
  errors,
  classNames,
  disabled,
}) => {
  const [length, setLength] = useState(value?.length || 0);
  let change = onChange ? onChange : (event) => setLength(event.target.value.length);

  const classes = clsx(
    'relative',
    // 'block w-full px-3 py-2 text-sm leading-tight text-gray-800 placeholder-gray-400 transition duration-100 ease-in-out bg-white focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 focus:outline-none focus:ring-opacity-50',
    classNames
  );

  return (
    <div className="relative flex flex-grow">
      <textarea
        name={name}
        disabled={disabled}
        id={name}
        rows={rows}
        type="textarea"
        defaultValue={value}
        ref={inputRef}
        maxLength={limit}
        placeholder={placeholder}
        className={classes}
        onChange={change}
        {...register(name)}
      ></textarea>
      {length > 0 && (
        <span className="absolute bottom-0 text-xs text-gray-400 right-4">{limit - length} characters left</span>
      )}
      <ErrorMessage errors={errors} name={name} as={ErrorMessageTag} />
    </div>
  );
};

LimitedTextarea.propTypes = {
  /**
   * The property name used by react hook form
   */
  name: PropTypes.string,
  /**
   * The help text to display
   */
  placeholder: PropTypes.string,
  /**
   * The value to preset the input to
   */
  value: PropTypes.string,
  /**
   * The number of rows to have in the textarea
   */
  rows: PropTypes.string,
  /**
   * The character count limit
   */
  limit: PropTypes.number.isRequired,
  /**
   * The ref property for use with registering with react hook form
   */
  inputRef: PropTypes.oneOfType([PropTypes.object, PropTypes.func]),
  /**
   * The function to execute when the text area value changes
   */
  onChange: PropTypes.func,
};

LimitedTextarea.defaultProps = {
  name: null,
  placeholder: null,
  value: '',
  rows: 3,
  limit: 500,
  inputRef: null,
  onChange: undefined,
  disabled: false,
};

export const LimitedTextareaFileInput = () => {
  const [file, setFile] = useState(null);
  const register = () => {};
  const formState = { errors: {} };
  const labelClasses = clsx('flex items-center bg-gray-800 rounded-r justify-center', {
    'flex-col': file,
  });

  return (
    <div className="flex flex-col">
      <Label id="injectateCharacterization" />
      <div className="flex flex-row">
        <LimitedTextarea
          name="injectateCharacterization"
          rows="5"
          limit={2500}
          register={register}
          disabled={file}
          placeholder={
            !file
              ? 'Type your characterization or upload a document'
              : 'This field is disabled when an attachment is chosen'
          }
          errors={formState.errors}
          className="border-0 border-t border-b border-r rounded-l rounded-r shadow-none"
        />
        <label className={labelClasses}>
          {file ? (
            <>
              <CheckIcon className="w-8 h-8 mx-2 text-green-500" />
              <XIcon className="w-8 h-8 mx-2 text-pink-500" />
            </>
          ) : (
            <>
              <CloudUploadIcon className="w-8 h-8 mx-2 text-white" />
              <input
                name="injectateCharacterizationFile"
                type="file"
                className="hidden"
                {...register}
                onChange={(e) => setFile(e.target.value)}
              />
            </>
          )}
        </label>
      </div>
    </div>
  );
};

export const useMaxLength = ({ value, limit }) => {
  const [length, setLength] = useState(value?.length || 0);
  const change = (event) => setLength(event.target.value.length);

  return {
    change,
    limit,
    remaining: limit - length,
  };
};

export const DropzoneMessaging = ({ isDragActive, files = [], reset = () => {} }) => {
  if (isDragActive) {
    return <p className="self-center">Ok, drop it!</p>;
  }

  if (files.length > 0) {
    return (
      <div className="">
        <div className="flex flex-row">
          <CheckIcon className="w-8 h-8 mx-2 text-green-500" />
          <span className="self-center overflow-hidden lowercase truncate text-gray-50 whitespace-nowrap">
            {files[0].name}
          </span>
        </div>
        <button type="button" className="w-full mt-4" onClick={reset}>
          <XIcon className="w-6 h-6 mx-2 text-pink-500" />
          <span className="self-center justify-between">Clear</span>
        </button>
      </div>
    );
  }

  return <p className="self-center text-center">Drop a file here or</p>;
};

const acceptableFileTypes = ['.pdf', '.doc', '.docx', '.jpeg', '.jpg', '.png'];

export const LimitedDropzone = ({ textarea, forms, file }) => {
  const [files, setFiles] = useState([]);
  const { limit, change, remaining } = useMaxLength({ limit: textarea.limit });
  const { getRootProps, getInputProps, isDragActive, open } = useDropzone({
    noClick: true,
    noKeyboard: true,
    multiple: false,
    maxFiles: 1,
    accept: acceptableFileTypes.join(),
    onDropRejected: () => {
      toast.error('File type not accepted');
    },
    onDrop: setFiles,
  });

  return (
    <section className="grid grid-cols-2" {...getRootProps()}>
      <Label className="col-span-2" id={textarea.id} />
      <section
        className={clsx('relative', {
          hidden: files.length > 0,
          'col-span-2': remaining < limit,
        })}
      >
        <textarea
          className="rounded-l"
          id={textarea.id}
          name={textarea.id}
          rows={textarea.rows}
          disabled={textarea.disabled}
          type="textarea"
          placeholder={textarea.placeholder}
          defaultValue={textarea.value}
          maxLength={limit}
          onKeyUp={change}
          {...forms.register(textarea.id)}
        />
        {remaining !== limit && (
          <span
            className={clsx('absolute bottom-0 text-sm right-5', {
              'text-gray-500': remaining > 10,
              'text-yellow-600': remaining <= 15 && remaining > 5,
              'text-red-600': remaining <= 5,
            })}
          >
            {remaining} characters left
          </span>
        )}
      </section>
      <section
        className={clsx('flex border-t border-b border-r rounded-r p-2', {
          'col-span-2': files.length > 0,
          hidden: remaining < limit,
        })}
      >
        <div
          className={clsx(
            'flex flex-col justify-around flex-grow px-2 border border-gray-800 border-dashed rounded-lg',
            {
              'bg-gray-50': files.length === 0,
              'bg-gray-800': files.length > 0,
            }
          )}
        >
          <input {...getInputProps()} {...forms.register(file.id)} />
          <DropzoneMessaging isDragActive={isDragActive} files={files} reset={() => setFiles([])} />
          {files.length === 0 && (
            <button type="button" onClick={open}>
              <CloudUploadIcon className="w-6 h-6 mx-2 text-white" />
              Choose File
            </button>
          )}
        </div>
      </section>
      <ErrorMessage errors={forms.errors} name={textarea.id} as={ErrorMessageTag} />
    </section>
  );
};
