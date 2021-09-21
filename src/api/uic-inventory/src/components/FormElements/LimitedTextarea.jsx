import { useState } from 'react';
import { Controller } from 'react-hook-form';
import { ErrorMessage } from '@hookform/error-message';
import clsx from 'clsx';
import PropTypes from 'prop-types';
import { CloudUploadIcon, XIcon, CheckIcon } from '@heroicons/react/outline';
import { useDropzone } from 'react-dropzone';
import { toast } from 'react-toastify';
import ErrorMessageTag from './ErrorMessage';
import { Label } from './TextInput';

export const LimitedTextarea = ({ id, rows, placeholder, value, maxLength, register, errors, className, disabled }) => {
  const { limit, change, remaining } = useMaxLength({ limit: maxLength });
  const { onChange, onBlur, ref, name } = register(id);

  return (
    <div className="relative flex flex-grow">
      <textarea
        name={name}
        disabled={disabled}
        id={name}
        rows={rows.toString()}
        type="textarea"
        defaultValue={value}
        ref={ref}
        maxLength={limit}
        placeholder={placeholder}
        className={className}
        onBlur={onBlur}
        onChange={(e) => {
          onChange(e);
          change(e);
        }}
      ></textarea>
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
        {forms.control && (
          <Controller
            name={textarea.id}
            control={forms?.control}
            render={({ field }) => (
              <textarea
                className="rounded-l"
                rows={textarea.rows}
                disabled={textarea.disabled}
                type="textarea"
                placeholder={textarea.placeholder}
                defaultValue={textarea.value}
                maxLength={limit}
                {...field}
                onChange={(e) => {
                  change(e);
                  field.onChange(e);
                }}
              />
            )}
          />
        )}
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
          {forms.control && (
            <Controller
              name={file.id}
              control={forms?.control}
              render={({ field }) => (
                <input
                  {...getInputProps({
                    onChange: (e) => {
                      console.log(e);
                      field.onChange(e.target.files[0]);
                    },
                  })}
                />
              )}
            />
          )}
          <DropzoneMessaging
            isDragActive={isDragActive}
            files={files}
            reset={() => {
              forms.reset({ [file.id]: undefined });
              setFiles([]);
            }}
          />
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
